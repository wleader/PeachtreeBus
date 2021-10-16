using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peachtreebus.Tests
{
    [TestClass]
    public class QueueWriterExtensionsFixture
    {
        [TestMethod]
        public void WriteMessage_ForwardsToInterface()
        {
            var moqWriter = new Mock<IQueueWriter>();

            var message = new TestSagaMessage1();
            var notBefore = DateTime.UtcNow;

            moqWriter.Setup(w => w.WriteMessage("TestQueue", typeof(TestSagaMessage1), message, notBefore)).Verifiable();

            moqWriter.Object.WriteMessage("TestQueue", message, notBefore);

            moqWriter.Verify();
           
        }
    }
}
