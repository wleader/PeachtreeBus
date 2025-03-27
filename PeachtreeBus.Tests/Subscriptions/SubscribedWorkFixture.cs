using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Telemetry;
using PeachtreeBus.Tests.Fakes;
using PeachtreeBus.Tests.Telemetry;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Subscriptions
{
    /// <summary>
    /// Proves the behavior of Subscribed Work
    /// </summary>
    [TestClass]
    public class SubscribedWorkFixture
    {
        public class TestMessage : ISubscribedMessage;

        private SubscribedWork work = default!;
        private Mock<ILogger<SubscribedWork>> log = default!;
        private Mock<IMeters> meters = default!;
        private Mock<ISubscribedReader> reader = default!;
        private Mock<IBusDataAccess> dataAccess = default!;
        private SubscribedContext context = default!;
        private Mock<ISubscribedPipelineInvoker> pipelineInvoker = default!;

        [TestInitialize]
        public void TestInitialize()
        {
            log = new();
            meters = new();
            reader = new();
            dataAccess = new();

            context = TestData.CreateSubscribedContext();

            reader.Setup(r => r.GetNext(It.IsAny<SubscriberId>()))
                .ReturnsAsync(context);

            pipelineInvoker = new();

            work = new SubscribedWork(
                FakeClock.Instance,
                reader.Object,
                meters.Object,
                log.Object,
                dataAccess.Object,
                pipelineInvoker.Object);
        }

        /// <summary>
        /// Proves when no message
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Given_NoPendingMessages_When_DoWork_Then_ReturnFalse()
        {
            reader.Setup(r => r.GetNext(It.IsAny<SubscriberId>()))
                .ReturnsAsync((SubscribedContext)null!);

            work.SubscriberId = SubscriberId.New();
            var result = await work.DoWork();

            Assert.IsFalse(result);
        }

        /// <summary>
        /// Proves performance counters are invoked
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Given_AMessage_When_DoWork_Then_IncrementCounters()
        {
            work.SubscriberId = SubscriberId.New();
            var result = await work.DoWork();

            meters.Verify(c => c.StartMessage(), Times.Once);
            meters.Verify(c => c.FinishMessage(), Times.Once);
        }

        [TestMethod]
        public async Task Given_AMessage_When_DoWork_Then_Activity()
        {
            work.SubscriberId = SubscriberId.New();

            using var listener = new TestActivityListener(ActivitySources.Messaging);

            var result = await work.DoWork();

            var activity = listener.ExpectOneCompleteActivity();
            ReceiveActivityFixture.AssertActivity(activity, context, FakeClock.Instance.UtcNow);
        }

        /// <summary>
        /// Proves that a savepoint is always created
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Given_AMessage_When_DoWork_Then_CreatesSavepoint()
        {
            work.SubscriberId = SubscriberId.New();
            var result = await work.DoWork();

            dataAccess.Verify(d => d.CreateSavepoint("BeforeSubscriptionHandler"), Times.Once);
        }

        [TestMethod]
        public async Task Given_AMessage_When_DoWork_Then_PipelineInvoked()
        {
            List<string> invocations = [];

            dataAccess.Setup(d => d.CreateSavepoint("BeforeSubscriptionHandler"))
                .Callback(() => invocations.Add("Savepoint"));

            pipelineInvoker.Setup(p => p.Invoke(context))
                .Callback(() => invocations.Add("Pipeline"));

            reader.Setup(r => r.Complete(context))
                .Callback(() => invocations.Add("Complete"));

            var result = await work.DoWork();

            List<string> expected = ["Savepoint", "Pipeline", "Complete"];
            CollectionAssert.AreEqual(expected, invocations);

            dataAccess.Verify(d => d.RollbackToSavepoint(It.IsAny<string>()), Times.Never);
            reader.Verify(r => r.Fail(It.IsAny<SubscribedContext>(), It.IsAny<Exception>()), Times.Never);
        }

        [TestMethod]
        public async Task Given_PipelineWillThrow_When_DoWork_Then_ExceptionIsHandled()
        {
            List<string> invocations = [];

            var exception = new TestException();
            pipelineInvoker.Setup(i => i.Invoke(It.IsAny<SubscribedContext>())).Throws(exception);

            dataAccess.Setup(d => d.RollbackToSavepoint("BeforeSubscriptionHandler"))
                .Callback(() => invocations.Add("Rollback"));

            reader.Setup(r => r.Fail(context, exception))
                .Callback(() => invocations.Add("Fail"));

            var result = await work.DoWork();
            Assert.IsTrue(result);

            List<string> expected = ["Rollback", "Fail"];
            CollectionAssert.AreEqual(expected, invocations);
        }


        [TestMethod]
        public async Task Given_PipelineWillThrow_When_DoWork_Then_ActityHasException()
        {
            List<string> invocations = [];

            var exception = new TestException();
            pipelineInvoker.Setup(i => i.Invoke(It.IsAny<SubscribedContext>())).Throws(exception);

            var listener = new TestActivityListener(ActivitySources.Messaging);

            var result = await work.DoWork();

            var activity = listener.ExpectOneCompleteActivity();
            activity.AssertException(exception);
        }
    }
}
