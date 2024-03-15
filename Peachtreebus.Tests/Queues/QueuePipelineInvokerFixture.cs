﻿using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Queues;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peachtreebus.Tests.Queues
{
    [TestClass]
    public class QueuePipelineInvokerFixture
    {
        private Mock<IWrappedScopeFactory> _scopeFactory;
        private Mock<ISharedDatabase> _sharedDatabase;
        private Mock<IWrappedScope> _scope;
        private Mock<IShareObjectsBetweenScopes> _provider;
        private Mock<IQueuePipelineFactory> _pipelineFactory;
        private Mock<IQueuePipeline> _pipeline;
        private QueuePipelineInvoker _invoker;
        private InternalQueueContext _context;

        [TestInitialize]
        public void Init()
        {
            _scope = new();

            _scopeFactory = new();
            _scopeFactory.Setup(f => f.Create()).Returns(_scope.Object);

            _sharedDatabase = new();

            _provider = new();
            _provider.SetupGet(p => p.SharedDatabase).Returns((ISharedDatabase)null);
            
            _scope.Setup(s => s.GetInstance<IShareObjectsBetweenScopes>()).Returns(_provider.Object);

            _pipelineFactory = new();
            _scope.Setup(s => s.GetInstance<IQueuePipelineFactory>()).Returns(_pipelineFactory.Object);

            _pipeline = new();
            _pipelineFactory.Setup(f => f.Build()).Returns(_pipeline.Object);

            _invoker = new(_scopeFactory.Object, _sharedDatabase.Object);

            _context = new();
        }

        [TestMethod]
        public async Task When_Invoked_Then_PipelineIsInvoked()
        {
            
            _pipeline.Setup(p => p.Invoke(It.IsAny<QueueContext>()))
                .Callback<QueueContext>(c => Assert.IsTrue(ReferenceEquals(c, _context)));

            await _invoker.Invoke(_context);
        }

        [TestMethod]
        public async Task When_Invoked_Then_SharedDatabaseRefusesDispose()
        {
            // ensure that the shared database does not get disposed by the inner scope.

            _sharedDatabase.SetupGet(db => db.DenyDispose).Returns(false);

            // veryify that when the scope is disposed, the 
            // shared database object will not dispose.
            _scope.Setup(s => s.Dispose()).Callback(() =>
            {
                _sharedDatabase.VerifySet(db => db.DenyDispose = true, Times.Once);
                _sharedDatabase.VerifySet(db => db.DenyDispose = false, Times.Never);
            });

            await _invoker.Invoke(_context);

            // only one scope was created
            _scopeFactory.Verify(f => f.Create(), Times.Once);

            // verify that the scope was disposed.
            _scope.Verify(s => s.Dispose(), Times.Once);

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
                _scope.Verify(s => s.Dispose(), Times.Once);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public async Task Given_GetSharedDatabaseProviderWillThrow_When_Invoke_ScopeIsDisposed()
        {
            _scope.Setup(s => s.GetInstance<IShareObjectsBetweenScopes>()).Throws<TestException>();
            await VerifyScopeDisposedOnException();
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public async Task Given_GetPipelineFactoryWillThrow_When_Invoke_ScopeIsDisposed()
        {
            _scope.Setup(s => s.GetInstance<IQueuePipelineFactory>()).Throws<TestException>();
            await VerifyScopeDisposedOnException();
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public async Task Given_FactoryBuildWillThrow_When_Invoke_ScopeIsDisposed()
        {
            _pipelineFactory.Setup(f => f.Build()).Throws<TestException>();
            await VerifyScopeDisposedOnException();
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public async Task Given_PipelineInvokeWillThrow_When_Invoke_ScopeIsDisposed()
        {
            _pipeline.Setup(p => p.Invoke(It.IsAny<QueueContext>())).Throws<TestException>();
            await VerifyScopeDisposedOnException();
        }

        [TestMethod]
        public async Task When_Invoked_Then_SharedDatabaseProviderWasSetupBeforePipelineIsBuilt()
        {
            // the shared database must be setup for sharing with the inner scope
            // before the factory builds the pipeline,
            // so that the build pipeline will re-use the shared database.

            bool providerSet = false;
            
            _provider.SetupSet(p => p.SharedDatabase = It.IsAny<ISharedDatabase>())
                .Callback<ISharedDatabase>((db) => providerSet = true);

            _scope.Setup(s => s.GetInstance<IQueuePipelineFactory>())
                .Callback(() => Assert.IsTrue(providerSet))
                .Returns(_pipelineFactory.Object);

            _pipelineFactory.Setup(f => f.Build())
                .Callback(() => Assert.IsTrue(providerSet))
                .Returns(_pipeline.Object);

            await _invoker.Invoke(_context);
        }
    }
}