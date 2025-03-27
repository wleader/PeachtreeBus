using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using PeachtreeBus.Telemetry;
using System;
using System.Diagnostics;
using System.Linq;

namespace PeachtreeBus.Tests.Telemetry;

public static class ActivityAsserts
{
    public static Activity AssertOutgoingContext(this Activity activity, IOutgoingContext context) =>
        activity
            .AssertNotBefore(context.NotBefore)
            .AssertPriority(context.MessagePriority);

    public static Activity AssertHandlerType(this Activity activity, Type type) =>
        activity.AssertTag("peachtreebus.handler.type", type.FullName);

    public static Activity AssertPipelineType(this Activity activity, Type type) =>
        activity.AssertTag("peachtreebus.pipeline.type", type.FullName);

    public static Activity AssertStarted(this Activity activity)
    {
        Assert.AreNotEqual(default, activity.StartTimeUtc);
        Assert.AreEqual(
            DateTime.UtcNow.Ticks,
            activity.StartTimeUtc.Ticks,
            TimeSpan.FromSeconds(1).Ticks);
        return activity;
    }

    public static Activity AssertQueueContext(this Activity activity, QueueContext context) =>
        activity
            .AssertDestination(context.SourceQueue)
            .AssertIncomingContext(context);

    public static Activity AssertDestination(this Activity activity, QueueName queueName) =>
        activity.AssertTag("messaging.destination.name", queueName.ToString());


    public static Activity AssertIncomingContext(this Activity activity, IIncomingContext context) =>
        activity
            .AssertMessageId(context.MessageId.ToString())
            .AssertPriority(context.MessagePriority)
            .AssertMessageClass(context.MessageClass)
            .AssertEnqueued(context.EnqueuedTime)
            .AssertNotBefore(context.NotBefore);

    public static Activity AssertNotBefore(this Activity activity, UtcDateTime expected) =>
        activity.AssertTag("peachtreebus.message.notbefore", expected.ToTagString());

    public static Activity AssertEnqueued(this Activity activity, UtcDateTime expected) =>
        activity.AssertTag("peachtreebus.message.enqueued", expected.ToTagString());

    public static Activity AssertMessageClass(this Activity activity, string expected) =>
        activity.AssertTag("peachtreebus.message.class", expected);

    public static Activity AssertPriority(this Activity activity, int expected) =>
        activity.AssertTag("peachtreebus.message.priority", expected.ToString());

    public static Activity AssertMessageId(this Activity activity, string expected) =>
        activity.AssertTag("messaging.message.id", expected);

    public static Activity AssertMessagingClientId(this Activity activity) =>
        activity.AssertTag("messaging.client.id", Environment.MachineName);

    public static Activity AssertMessagingOperation(this Activity activity, string expected) =>
        activity.AssertTag("messaging.operation.type", expected);

    public static Activity AssertMessagingSystem(this Activity activity) =>
        activity.AssertTag("messaging.system", "peachtreebus");

    public static Activity AssertTag(this Activity activity, string key, string? value)
    {
        var tag = activity.Tags.SingleOrDefault(t => t.Key == key);
        // KeyValuePair is a struct, so it won't be null,
        // but it will have a null key when not found.
        Assert.AreEqual(tag.Key, key, $"Tag not found: {key}");
        Assert.AreEqual(value, tag.Value, "Tag value incorrect.");
        return activity;
    }

    public static Activity AssertStartTime(this Activity activity, DateTime expected)
    {
        Assert.AreEqual(expected, activity.StartTimeUtc);
        return activity;
    }

    public static Activity AssertKind(this Activity activity, ActivityKind expected)
    {
        Assert.AreEqual(expected, activity.Kind);
        return activity;
    }

    public static Activity AssertOperationName(this Activity activity, string expected)
    {
        Assert.AreEqual(expected, activity.OperationName);
        return activity;
    }

    public static T AssertIsNotNull<T>(this T? value)
    {
        Assert.IsNotNull(value);
        return value;
    }
}
