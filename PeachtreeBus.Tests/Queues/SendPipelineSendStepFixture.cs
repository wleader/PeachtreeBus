using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Queues
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

        private object userMessage = default!;

        private ISendContext? invokedContext = default;

        [TestInitialize]
        public void TestInitialize()
        {
            pipelineInvoker = new();

            pipelineInvoker.Setup(x => x.Invoke(It.IsAny<ISendContext>()))
                .Callback((ISendContext c) => invokedContext = c);

            userMessage = TestData.CreateQueueUserMessage();

            writer = new QueueWriter(pipelineInvoker.Object);
        }

        /// <summary>
        /// Proves the message cannot be null.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Given_MessageIsNull_When_WriteMessage_Then_Throws()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
                writer.WriteMessage(
                    TestData.DefaultQueueName,
                    userMessage.GetType(),
                    null!));
        }

        /// <summary>
        /// Proves the type cannot be null
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Given_TypeIsNull_When_WriteMessage_Then_Throws()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
                writer.WriteMessage(
                    TestData.DefaultQueueName,
                    null!,
                    userMessage));
        }

        /// <summary>
        /// proves the message class is set.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Given_Type_When_WriteMessage_Then_ContextTypeIsSet()
        {
            await writer.WriteMessage(
                TestData.DefaultQueueName,
                userMessage.GetType(),
                userMessage);
            Assert.AreEqual(userMessage.GetType(), invokedContext?.Type);
        }

        /// <summary>
        /// Proves that NotBefore defaults to Now
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Given_NullNotBefore_When_WriteMessage_ThenContextNotBeforeNull()
        {
            await writer.WriteMessage(
                TestData.DefaultQueueName,
                userMessage.GetType(),
                userMessage,
                notBefore: null);

            Assert.IsNotNull(invokedContext);
            Assert.IsFalse(invokedContext.NotBefore.HasValue);
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
                userMessage.GetType(),
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
                userMessage.GetType(),
                userMessage);
            Assert.AreEqual(expected, invokedContext?.Destination);
        }

        [TestMethod]
        public async Task Given_MessageIsNotIQueuedMessage_When_WriteMessage_Then_ThrowsUsefulException()
        {
            await Assert.ThrowsExceptionAsync<TypeIsNotIQueueMessageException>(() =>
                writer.WriteMessage(new("FooBazQueue"),
                typeof(object),
                new object()));
        }

        [TestMethod]
        public async Task Given_Priority_When_Publish_Then_PriorityIsSet()
        {
            await writer.WriteMessage(
                TestData.DefaultQueueName,
                userMessage.GetType(),
                userMessage,
                priority: 100);

            Assert.AreEqual(100, invokedContext?.Priority);
        }

        [TestMethod]
        public async Task Given_UserHeaders_When_Publish_Then_UserHeadersAreUsed()
        {
            await writer.WriteMessage(
                TestData.DefaultQueueName,
                userMessage.GetType(),
                userMessage,
                userHeaders: TestData.DefaultUserHeaders);

            Assert.AreSame(TestData.DefaultUserHeaders, invokedContext?.UserHeaders);
        }
    }
}
