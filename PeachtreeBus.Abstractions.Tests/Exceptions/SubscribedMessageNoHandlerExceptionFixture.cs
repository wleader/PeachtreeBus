﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using PeachtreeBus.Exceptions;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Abstractions.Tests.Exceptions;

[TestClass]
public class SubscribedMessageNoHandlerExceptionFixture
{
    public class TestMessage : ISubscribedMessage { }

    [TestMethod]
    public void When_New_Then_PropertiesAreSet()
    {
        var expectedMessageId = UniqueIdentity.New();
        var expectedSubscriberId = SubscriberId.New();
        var expectedType = typeof(TestMessage);
        var e = new SubscribedMessageNoHandlerException(
            expectedMessageId, expectedSubscriberId, expectedType);
        Assert.AreEqual(expectedMessageId, e.MessageId);
        Assert.AreEqual(expectedSubscriberId, e.SubscriberId);
        Assert.AreEqual(expectedType, e.MessageType);
    }
}
