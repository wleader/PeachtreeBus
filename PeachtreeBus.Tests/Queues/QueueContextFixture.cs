using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using System;

namespace PeachtreeBus.Tests.Queues;

[TestClass]
public class QueueContextFixture
{
    [TestMethod]
    public void Given_QueueContext_When_GetEnqueuedTime_Then_Value()
    {
        UtcDateTime enqueuedTime = DateTime.UtcNow;

        IQueueContext context = new QueueContext()
        {
            Message = new object(),
            MessageData = new()
            {
                Body = new("BODY"),
                Enqueued = enqueuedTime,
                Headers = new("HEADERS"),
                MessageId = UniqueIdentity.New(),
                NotBefore = DateTime.UtcNow,
                Priority = 0
            }
        };

        Assert.AreEqual(enqueuedTime, context.EnqueuedTime);
    }

    [TestMethod]
    public void Given_QueueContext_When_GetMessageId_Then_Value()
    {
        var messageId = UniqueIdentity.New();

        IQueueContext context = new QueueContext()
        {
            Message = new object(),
            MessageData = new()
            {
                Body = new("BODY"),
                Enqueued = DateTime.UtcNow,
                Headers = new("HEADERS"),
                MessageId = messageId,
                NotBefore = DateTime.UtcNow,
                Priority = 0
            }
        };

        Assert.AreEqual(messageId, context.MessageId);
    }
}
