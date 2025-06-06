﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.ClassNames;
using PeachtreeBus.Core.Tests;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Queues
{
    /// <summary>
    /// Proves the behavior of QueueWriter
    /// </summary>
    [TestClass]
    public class QueueWriterFixture
    {
        public class MessageWithoutInterface { }

        private QueueWriter writer = default!;
        private Mock<ISendPipelineInvoker> pipelineInvoker = default!;
        private readonly Mock<ISystemClock> clock = new();
        private readonly ClassNameService classNameService = new();

        private IQueueMessage userMessage = default!;

        private ISendContext? invokedContext = default;

        [TestInitialize]
        public void TestInitialize()
        {
            clock.Reset();
            clock.SetupGet(c => c.UtcNow).Returns(TestData.Now);

            pipelineInvoker = new();

            pipelineInvoker.Setup(x => x.Invoke(It.IsAny<SendContext>()))
                .Callback((ISendContext c) => invokedContext = c);

            userMessage = TestData.CreateQueueUserMessage();

            writer = new QueueWriter(
                clock.Object,
                pipelineInvoker.Object,
                classNameService);
        }

        /// <summary>
        /// Proves the message cannot be null.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Given_MessageIsNull_When_WriteMessage_Then_Throws()
        {
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
                writer.WriteMessage(
                    TestData.DefaultQueueName,
                    null!));
        }

        /// <summary>
        /// Proves the supplied NotBefore is used
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Given_NotBefore_When_WriteMessage_ThenContextNotBeforeIsSet()
        {
            UtcDateTime notBefore = DateTime.UtcNow;
            await writer.WriteMessage(
                TestData.DefaultQueueName,
                userMessage,
                notBefore: notBefore);

            Assert.AreEqual(notBefore, invokedContext?.NotBefore);
        }

        /// <summary>
        /// Proves the correct queue is used.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Given_Queue_When_WriteMessage_ContextDestinationIsSet()
        {
            var expected = new QueueName("FooBazQueue");
            await writer.WriteMessage(
                expected,
                userMessage);
            Assert.AreEqual(expected, invokedContext?.Destination);
        }

        [TestMethod]
        public async Task Given_Priority_When_Publish_Then_PriorityIsSet()
        {
            await writer.WriteMessage(
                TestData.DefaultQueueName,
                userMessage,
                priority: 100);

            Assert.AreEqual(100, invokedContext?.MessagePriority);
        }

        [TestMethod]
        public async Task Given_UserHeaders_When_Publish_Then_UserHeadersAreUsed()
        {
            await writer.WriteMessage(
                TestData.DefaultQueueName,
                userMessage,
                userHeaders: TestData.DefaultUserHeaders);

            Assert.AreSame(TestData.DefaultUserHeaders, invokedContext?.UserHeaders);
        }
    }
}
