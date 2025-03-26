using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Abstractions.Tests.TestClasses;
using PeachtreeBus.Data;
using PeachtreeBus.Exceptions;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;

namespace PeachtreeBus.Abstractions.Tests.Exceptions;

[TestClass]
public class SagaNotStartedExceptionFixture
{
    [TestMethod]
    public void When_New_Then_PropertiesAreSet()
    {
        var messageId = UniqueIdentity.New();
        var sourceQueue = new QueueName("TestQueue");
        var messageType = typeof(TestQueuedMessage);
        var sagaType = typeof(Saga<>);
        var sagaKey = new SagaKey("SagaKey");
        var e = new SagaNotStartedException(
            messageId, sourceQueue, messageType, sagaType, sagaKey);
        Assert.AreEqual(messageId, e.MessageId);
        Assert.AreEqual(sourceQueue, e.SourceQueue);
        Assert.AreEqual(messageType, e.MessageType);
        Assert.AreEqual(sagaType, e.SagaType);
        Assert.AreEqual(sagaKey, e.SagaKey);
    }
}
