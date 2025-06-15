using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.ClassNames;
using PeachtreeBus.Data;
using PeachtreeBus.Exceptions;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Abstractions.Tests.Exceptions;

[TestClass]
public class SubscribedMessageClassNotRecognizedExceptionFixture
{
    public class TestMessage : ISubscribedMessage { }

    [TestMethod]
    public void When_New_Then_PropertiesAreSet()
    {
        var expectedMessageId = UniqueIdentity.New();
        var expectedSubscriberId = SubscriberId.New();
        var expectedType = new ClassName("Foo");
        var e = new SubscribedMessageClassNotRecognizedException(
            expectedMessageId, expectedSubscriberId, expectedType);
        Assert.AreEqual(expectedMessageId, e.MessageId);
        Assert.AreEqual(expectedSubscriberId, e.SubscriberId);
        Assert.AreEqual(expectedType, e.ClassName);
    }
}
