using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Queues;
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
        private Mock<IQueueWriter> _writer = default!;

        [TestInitialize]
        public void Intialize()
        {
            _writer = new();
        }

        [TestMethod]
        public async Task WriteMessage_ForwardsToInterface()
        {
            var message = TestData.CreateQueueUserMessage();
            var notBefore = DateTime.UtcNow;

            _writer.Setup(w => w.WriteMessage(
                TestData.DefaultQueueName,
                message.GetType(),
                message,
                notBefore,
                10,
                TestData.DefaultUserHeaders
                )).Verifiable();

            await _writer.Object.WriteMessage(
                TestData.DefaultQueueName,
                message,
                notBefore,
                10,
                TestData.DefaultUserHeaders);

            _writer.Verify();

        }
    }
}
