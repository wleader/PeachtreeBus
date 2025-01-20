using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Queues;
using System;

namespace PeachtreeBus.Tests.Queues;

[TestClass]
public class QueueMessageClassNotRecognizedExceptionFixture
{
    [TestMethod]
    public void When_New_Then_PropertiesAreSet()
    {
        var expectedMessageId = Guid.NewGuid();
        var expectedQueueName = new QueueName("TestQueue");
        var expectedType = "Type Name";
        var e = new QueueMessageClassNotRecognizedException(expectedMessageId, expectedQueueName, expectedType);
        Assert.AreEqual(expectedMessageId, e.MessageId);
        Assert.AreEqual(expectedQueueName, e.SourceQueue);
        Assert.AreEqual(expectedType, e.TypeName);
    }
}
