using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Tests.Subscriptions;

[TestClass]
public class SubscribedMessageClassNotRecognizedExceptionFixture
{
    public class TestMessage : ISubscribedMessage { }

    [TestMethod]
    public void When_New_Then_PropertiesAreSet()
    {
        var expectedMessageId = UniqueIdentity.New();
        var expectedSubscriberId = SubscriberId.New();
        var expectedType = typeof(TestMessage).FullName;
        var e = new SubscribedMessageClassNotRecognizedException(expectedMessageId, expectedSubscriberId, expectedType);
        Assert.AreEqual(expectedMessageId, e.MessageId);
        Assert.AreEqual(expectedSubscriberId, e.SubscriberId);
        Assert.AreEqual(expectedType, e.TypeName);
    }
}
