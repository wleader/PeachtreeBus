using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Errors
{
    [TestClass]
    public class SubscribedFailuresFixture
    {
        private SubscribedFailures _testSubject = default!;

        private Mock<ILogger<SubscribedFailures>> log = default!;
        private Mock<IBusDataAccess> dataAccess = default!;
        private Mock<IHandleFailedSubscribedMessages> handler = default!;

        [TestInitialize]
        public void Initialize()
        {
            log = new();
            dataAccess = new();
            handler = new();

            _testSubject = new SubscribedFailures(log.Object, dataAccess.Object, handler.Object);
        }

        [TestMethod]
        public async Task When_Failed_Then_HandlerInvoked()
        {
            var context = TestData.CreateSubscribedContext();
            var exception = new ApplicationException();

            await _testSubject.Failed(context, context.Message, exception);

            dataAccess.Verify(d => d.CreateSavepoint(It.IsAny<string>()), Times.Once());

            handler.Verify(h => h.Handle(context, context.Message, exception), Times.Once());

            dataAccess.Verify(d => d.RollbackToSavepoint(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task Given_HandlerWillThrow_When_Failed_Then_Rollback()
        {
            var context = TestData.CreateSubscribedContext();
            var exception = new ApplicationException();

            handler.Setup(h => h.Handle(
                It.IsAny<SubscribedContext>(),
                It.IsAny<object>(),
                It.IsAny<Exception>()))
                .Throws(new ApplicationException());

            await _testSubject.Failed(context, context.Message, exception);

            dataAccess.Verify(d => d.CreateSavepoint(It.IsAny<string>()), Times.Once());

            handler.Verify(h => h.Handle(context, context.Message, exception), Times.Once());

            dataAccess.Verify(d => d.RollbackToSavepoint(It.IsAny<string>()), Times.Once());
        }
    }
}
