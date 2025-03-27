using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Queues;
using PeachtreeBus.Telemetry;
using System;
using System.Diagnostics;
using System.Linq;

namespace PeachtreeBus.Tests.Telemetry;

[TestClass]
public class ReceiveActivityFixture()
    : ActivityFixtureBase(ActivitySources.Messaging)
{
    [TestMethod]
    public void When_Activity_Then_TagsAreCorrect()
    {
        var context = TestData.CreateQueueContext();
        var started = DateTime.UtcNow;
        new ReceiveActivity(context, started).Dispose();

        Assert(_listener.Stopped.SingleOrDefault(), context, started);
    }

    public static void Assert(Activity? activity, QueueContext context, DateTime started) =>
        activity.AssertIsNotNull()
            .AssertOperationName("receive " + context.SourceQueue.ToString())
            .AssertKind(ActivityKind.Client)
            .AssertStartTime(started)
            .AssertMessagingSystem()
            .AssertMessagingOperation("receive")
            .AssertMessagingClientId()
            .AssertIncomingContext(context)
            .AssertStarted();
}
