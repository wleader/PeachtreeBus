using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using PeachtreeBus.Exceptions;
using PeachtreeBus.Queues;

namespace PeachtreeBus.Tests.Queues;

[TestClass]
public class QueueMessageClassNotRecognizedExceptionFixture
{
    [TestMethod]
    public void When_New_Then_PropertiesAreSet()
    {
        var expectedMessageId = UniqueIdentity.New();
        var expectedQueueName = new QueueName("TestQueue");
        var expectedType = "Type Name";
        var e = new QueueMessageClassNotRecognizedException(
            expectedMessageId, expectedQueueName, expectedType);
        Assert.AreEqual(expectedMessageId, e.MessageId);
        Assert.AreEqual(expectedQueueName, e.SourceQueue);
        Assert.AreEqual(expectedType, e.TypeName);
    }
}
