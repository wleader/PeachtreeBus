using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Peachtreebus.Tests.Sagas;
using PeachtreeBus.Queues;
using System;
using System.Threading.Tasks;

namespace Peachtreebus.Tests.Queues
{
    /// <summary>
    /// Proves the behavior of QueueWriterExtensions
    /// </summary>
    [TestClass]
    public class QueueWriterExtensionsFixture
    {
        /// <summary>
        /// Proves WriteMessage, writes the message.
        /// </summary>
        [TestMethod]
        public async Task WriteMessage_ForwardsToInterface()
        {
            var writer = new Mock<IQueueWriter>();

            var message = new TestSagaMessage1();
            var notBefore = DateTime.UtcNow;

            writer.Setup(w => w.WriteMessage("TestQueue", typeof(TestSagaMessage1), message, notBefore)).Verifiable();

            await writer.Object.WriteMessage("TestQueue", message, notBefore);

            writer.Verify();

        }
    }
}
