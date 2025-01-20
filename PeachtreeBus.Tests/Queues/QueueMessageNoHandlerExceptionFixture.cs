using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Queues;
using PeachtreeBus.Tests.Sagas;
using System;

namespace PeachtreeBus.Tests.Queues;

[TestClass]
public class QueueMessageNoHandlerExceptionFixture
{
    [TestMethod]
    public void When_New_Then_PropertiesAreSet()
    {
        var expectedMessageId = Guid.NewGuid();
        var expectedQueueName = new QueueName("TestQueue");
        var expectedType = typeof(TestSagaMessage1);
        var e = new QueueMessageNoHandlerException(expectedMessageId, expectedQueueName, expectedType);
        Assert.AreEqual(expectedMessageId, e.MessageId);
        Assert.AreEqual(expectedQueueName, e.SourceQueue);
        Assert.AreEqual(expectedType, e.MessageType);
    }
}
