using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using System;

namespace PeachtreeBus.Tests.Sagas;

[TestClass]
public class SagaNotStartedExceptionFixture
{
    [TestMethod]
    public void When_New_Then_PropertiesAreSet()
    {
        var messageId = Guid.NewGuid();
        var sourceQueue = new QueueName("TestQueue");
        var messageType = typeof(TestSagaMessage1);
        var sagaType = typeof(TestSaga);
        var sagaKey = "SagaKey";
        var e = new SagaNotStartedException(messageId, sourceQueue, messageType, sagaType, sagaKey);
        Assert.AreEqual(messageId, e.MessageId);
        Assert.AreEqual(sourceQueue, e.SourceQueue);
        Assert.AreEqual(messageType, e.MessageType);
        Assert.AreEqual(sagaType, e.SagaType);
        Assert.AreEqual(sagaKey, e.SagaKey);
    }
}
