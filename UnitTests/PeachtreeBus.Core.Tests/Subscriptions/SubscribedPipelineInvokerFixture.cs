using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Testing;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Subscriptions
{
    [TestClass]
    public class SubscribedPipelineInvokerFixture
    {
        private Mock<IScopeFactory> _scopeFactory = default!;
        private Mock<ISharedDatabase> _sharedDatabase = default!;
        private readonly FakeServiceProviderAccessor _accessor = new(new());
        private Mock<IShareObjectsBetweenScopes> _shareObjects = default!;
        private Mock<ISubscribedPipelineFactory> _pipelineFactory = default!;
        private Mock<ISubscribedPipeline> _pipeline = default!;
        private SubscribedPipelineInvoker _invoker = default!;
        private SubscribedContext _context = default!;
        private BusContextAccessor _contextAccessor = new();

        [TestInitialize]
        public void Init()
        {
            _accessor.Reset();

            _scopeFactory = new();
            _scopeFactory.Setup(f => f.Create()).Returns(_accessor.Object);

            _sharedDatabase = new();

            _shareObjects = new();
            _shareObjects.SetupGet(p => p.SharedDatabase).Returns((ISharedDatabase)null!);
            _accessor.AddMock(_shareObjects);

            _pipelineFactory = new();
            _accessor.AddMock(_pipelineFactory);

            _pipeline = new();
            _pipelineFactory.Setup(f => f.Build(It.IsAny<SubscribedContext>())).Returns(_pipeline.Object);

            _invoker = new(_scopeFactory.Object, _sharedDatabase.Object);

            _context = TestData.CreateSubscribedContext();
        }

        [TestMethod]
        public async Task When_Invoked_Then_PipelineIsInvoked()
        {
            _pipeline.Setup(p => p.Invoke(It.IsAny<ISubscribedContext>()))
                .Callback<ISubscribedContext>(c => Assert.IsTrue(ReferenceEquals(c, _context)));

            await _invoker.Invoke(_context);
        }

        [TestMethod]
        public async Task When_Invoked_Then_ContextAccessorIsSet()
        {
            _pipeline.Setup(p => p.Invoke(It.IsAny<ISubscribedContext>()))
                .Callback<ISubscribedContext>(c => Assert.IsTrue(ReferenceEquals(c, _contextAccessor.SubscribedContext)));

            await _invoker.Invoke(_context);
        }

        [TestMethod]
        public async Task When_Invoked_Then_ContextScopeIsSet()
        {
            await _invoker.Invoke(_context);
            Assert.AreSame(_accessor.ServiceProvider, _context.ServiceProvider);
        }

        [TestMethod]
        public async Task When_Invoked_Then_SharedDatabaseRefusesDispose()
        {
            // ensure that the shared database does not get disposed by the inner scope.

            _sharedDatabase.SetupGet(db => db.DenyDispose).Returns(false);

            // veryify that when the scope is disposed, the 
            // shared database object will not dispose.
            _accessor.Mock.Setup(s => s.Dispose()).Callback(() =>
            {
                _sharedDatabase.VerifySet(db => db.DenyDispose = true, Times.Once);
                _sharedDatabase.VerifySet(db => db.DenyDispose = false, Times.Never);
            });

            await _invoker.Invoke(_context);

            // only one scope was created
            _scopeFactory.Verify(f => f.Create(), Times.Once);

            // verify that the scope was disposed.
            _accessor.Mock.Verify(s => s.Dispose(), Times.Once);

            // verify that after there dispose, then deny dispose is set back to false.
            _sharedDatabase.VerifySet(db => db.DenyDispose = false, Times.Once);
        }

        private async Task VerifyScopeDisposedOnException()
        {
            try
            {
                await _invoker.Invoke(_context);
            }
            catch (Exception)
            {
                _accessor.Mock.Verify(s => s.Dispose(), Times.Once);
                throw;
            }
        }

        [TestMethod]
        public async Task Given_NothingThrows_When_Invoke_ScopeIsDisposed()
        {
            await VerifyScopeDisposedOnException();
            _accessor.Mock.Verify(s => s.Dispose(), Times.Once);
        }

        [TestMethod]
        public async Task Given_GetSharedDatabaseProviderWillThrow_When_Invoke_ScopeIsDisposed()
        {
            _accessor.SetupThrow<IShareObjectsBetweenScopes, TestException>();
            await Assert.ThrowsExactlyAsync<TestException>(VerifyScopeDisposedOnException);
        }

        [TestMethod]
        public async Task Given_GetPipelineFactoryWillThrow_When_Invoke_ScopeIsDisposed()
        {
            _accessor.SetupThrow<ISubscribedPipelineFactory, TestException>();
            await Assert.ThrowsExactlyAsync<TestException>(VerifyScopeDisposedOnException);
        }

        [TestMethod]
        public async Task Given_FactoryBuildWillThrow_When_Invoke_ScopeIsDisposed()
        {
            _pipelineFactory.Setup(f => f.Build(It.IsAny<SubscribedContext>())).Throws<TestException>();
            await Assert.ThrowsExactlyAsync<TestException>(VerifyScopeDisposedOnException);
        }

        [TestMethod]
        public async Task Given_PipelineInvokeWillThrow_When_Invoke_ScopeIsDisposed()
        {
            _pipeline.Setup(p => p.Invoke(It.IsAny<SubscribedContext>())).Throws<TestException>();
            await Assert.ThrowsExactlyAsync<TestException>(VerifyScopeDisposedOnException);
        }

        [TestMethod]
        public async Task When_Invoked_Then_SharedDatabaseProviderWasSetupBeforePipelineIsBuilt()
        {
            // the shared database must be setup for sharing with the inner scope
            // before the factory builds the pipeline,
            // so that the build pipeline will re-use the shared database.

            bool providerSet = false;

            _shareObjects.SetupSet(p => p.SharedDatabase = It.IsAny<ISharedDatabase>())
                .Callback<ISharedDatabase>((db) => providerSet = true);

            _accessor.Add(_pipelineFactory.Object, () => Assert.IsTrue(providerSet));

            _pipelineFactory.Setup(f => f.Build(It.IsAny<SubscribedContext>()))
                .Callback(() => Assert.IsTrue(providerSet))
                .Returns(_pipeline.Object);

            await _invoker.Invoke(_context);
        }
    }
}
