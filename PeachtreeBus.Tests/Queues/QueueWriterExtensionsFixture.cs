using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Queues;
using PeachtreeBus.Tests.Sagas;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Queues
{
    /// <summary>
    /// Proves the behavior of QueueWriterExtensions
    /// </summary>
    [TestClass]
    public class QueueWriterExtensionsFixture
    {
        private readonly QueueName TestQueue = new("TestQueue");

        /// <summary>
        /// Proves WriteMessage, writes the message.
        /// </summary>
        [TestMethod]
        public async Task WriteMessage_ForwardsToInterface()
        {
            var writer = new Mock<IQueueWriter>();

            var message = new TestSagaMessage1();
            var notBefore = DateTime.UtcNow;

            writer.Setup(w => w.WriteMessage(TestQueue, typeof(TestSagaMessage1), message, notBefore, 10)).Verifiable();

            await writer.Object.WriteMessage(TestQueue, message, notBefore, 10);

            writer.Verify();

        }
    }
}
