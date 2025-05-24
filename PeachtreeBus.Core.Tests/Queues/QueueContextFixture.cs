using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.ClassNames;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using System;

namespace PeachtreeBus.Core.Tests.Queues;

[TestClass]
public class QueueContextFixture
{
    [TestMethod]
    public void Given_QueueContext_Then_PropertiesPassThrough()
    {
        UtcDateTime enqueued = DateTime.UtcNow;
        UtcDateTime notBefore = enqueued.AddMinutes(1);
        var messageId = UniqueIdentity.New();
        Headers headers = new () { MessageClass = ClassName.Default };
        int priority = 23;

        var context = new QueueContext()
        {
            Message = new object(),
            Data = new()
            {
                Body = new("BODY"),
                Enqueued = enqueued,
                Headers = headers,
                MessageId = messageId,
                NotBefore = notBefore,
                Priority = priority,
            },
        };

        Assert.AreEqual(enqueued, context.EnqueuedTime);
        Assert.AreEqual(notBefore, context.NotBefore);
        Assert.AreEqual(messageId, context.MessageId);
        Assert.AreSame(headers, context.Headers);
        Assert.AreEqual(priority, context.MessagePriority);
        Assert.AreEqual(ClassName.Default, context.MessageClass);
        Assert.AreSame(headers.UserHeaders, context.UserHeaders);
    }
}
