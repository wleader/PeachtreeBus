using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Subscriptions;
using System;

namespace PeachtreeBus.Tests.Subscriptions;

[TestClass]
public class SubscribedMessageNoHandlerExceptionFixture
{
    public class TestMessage : ISubscribedMessage { }

    [TestMethod]
    public void When_New_Then_PropertiesAreSet()
    {
        var expectedMessageId = Guid.NewGuid();
        var expectedSubscriberId = Guid.NewGuid();
        var expectedType = typeof(TestMessage);
        var e = new SubscribedMessageNoHandlerException(expectedMessageId, expectedSubscriberId, expectedType);
        Assert.AreEqual(expectedMessageId, e.MessageId);
        Assert.AreEqual(expectedSubscriberId, e.SubscriberId);
        Assert.AreEqual(expectedType, e.MessageType);
    }
}
