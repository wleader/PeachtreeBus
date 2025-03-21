using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using PeachtreeBus.Exceptions;
using PeachtreeBus.Queues;

namespace PeachtreeBus.Abstractions.Tests.Exceptions;

[TestClass]
public class QueueMessageNoHandlerExceptionFixture
{
    [TestMethod]
    public void When_New_Then_PropertiesAreSet()
    {
        var expectedMessageId = UniqueIdentity.New();
        var expectedQueueName = new QueueName("TestQueue");
        var expectedType = typeof(AbstractionsTestData.TestQueuedMessage);
        var e = new QueueMessageNoHandlerException(
            expectedMessageId, expectedQueueName, expectedType);
        Assert.AreEqual(expectedMessageId, e.MessageId);
        Assert.AreEqual(expectedQueueName, e.SourceQueue);
        Assert.AreEqual(expectedType, e.MessageType);
    }
}
