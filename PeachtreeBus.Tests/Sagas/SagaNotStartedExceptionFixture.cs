using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using PeachtreeBus.Exceptions;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;

namespace PeachtreeBus.Tests.Sagas;

[TestClass]
public class SagaNotStartedExceptionFixture
{
    [TestMethod]
    public void When_New_Then_PropertiesAreSet()
    {
        var messageId = UniqueIdentity.New();
        var sourceQueue = new QueueName("TestQueue");
        var messageType = typeof(TestSagaMessage1);
        var sagaType = typeof(TestSaga);
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
