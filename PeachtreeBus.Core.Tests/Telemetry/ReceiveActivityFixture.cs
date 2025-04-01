using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
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
    public void Given_QueueContext_When_Activity_Then_TagsAreCorrect()
    {
        var context = TestData.CreateQueueContext();
        var started = DateTime.UtcNow;
        new ReceiveActivity(context, started).Dispose();

        AssertActivity(_listener.Stopped.SingleOrDefault(), context, started);
    }

    public static void AssertActivity(Activity? activity, QueueContext context, DateTime started) =>
        activity.AssertIsNotNull()
            .AssertOperationName("receive " + context.SourceQueue.ToString())
            .AssertKind(ActivityKind.Client)
            .AssertStartTime(started)
            .AssertMessagingSystem()
            .AssertMessagingOperation("receive")
            .AssertMessagingClientId()
            .AssertQueueContext(context);

    [TestMethod]
    public void Given_SubscribedContext_When_Activity_Then_TagsAreCorrect()
    {
        var context = TestData.CreateSubscribedContext();
        var started = DateTime.UtcNow;
        new ReceiveActivity(context, started).Dispose();

        AssertActivity(_listener.Stopped.SingleOrDefault(), context, started);
    }

    public static void AssertActivity(Activity? activity, SubscribedContext context, DateTime started) =>
    activity.AssertIsNotNull()
        .AssertOperationName("receive " + context.Topic.ToString())
        .AssertKind(ActivityKind.Client)
        .AssertStartTime(started)
        .AssertMessagingSystem()
        .AssertMessagingOperation("receive")
        .AssertMessagingClientId()
        .AssertSubscribedContext(context);
}
