using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peachtreebus.Tests
{
    [TestClass]
    public class MessageContextFixture
    {
        [TestMethod]
        public void Send_AddsToSentMessages()
        {
            var context = new MessageContext();
            var message = new object();
            var notBefore = DateTime.UtcNow.AddMinutes(1);

            Assert.AreEqual(0, context.SentMessages.Count);
            context.Send(message, "QueueName", notBefore);

            Assert.AreEqual(1, context.SentMessages.Count);

            Assert.AreEqual(notBefore, context.SentMessages[0].NotBefore);
            Assert.AreEqual("QueueName", context.SentMessages[0].QueueName);
            Assert.IsTrue(ReferenceEquals(message, context.SentMessages[0].Message));
            Assert.AreEqual(typeof(object), context.SentMessages[0].Type);
        }

        [TestMethod]
        public void Constructor_Initializes()
        {
            var context = new MessageContext();
            Assert.IsNotNull(context.SentMessages);
        }
    }
}
