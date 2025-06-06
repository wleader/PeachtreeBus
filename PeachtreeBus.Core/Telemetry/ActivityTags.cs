﻿using PeachtreeBus.ClassNames;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System;
using System.Diagnostics;

namespace PeachtreeBus.Telemetry;

public static class ActivityTags
{
    public static Activity? AddHandlerType(this Activity? activity, Type type) =>
        activity?.AddTag("peachtreebus.handler.type", type.GetTypeFullName());

    public static Activity? AddPipelineType(this Activity? activity, Type type) =>
        activity?.AddTag("peachtreebus.pipeline.type", type.GetTypeFullName());

    public static Activity? AddOutgoingContext(this Activity? activity, IOutgoingContext context) =>
        activity?.AddPriority(context.MessagePriority)
            ?.AddNotBefore(context.NotBefore)
            ?.AddMessageClass(context.MessageClass);

    public static Activity? AddMessageId(this Activity? activity, UniqueIdentity id) =>
        activity?.AddTag("messaging.message.id", id.ToString());

    public static Activity? AddPriority(this Activity? activity, int priority) =>
        activity?.AddTag("peachtreebus.message.priority", priority.ToString());

    public static Activity? AddEnqueued(this Activity? activity, UtcDateTime enqueued) =>
        activity?.AddTag("peachtreebus.message.enqueued", enqueued.ToTagString());

    public static Activity? AddMessageClass(this Activity? activity, ClassName className) =>
        activity?.AddTag("peachtreebus.message.class", className.Value);

    public static Activity? AddNotBefore(this Activity? activity, UtcDateTime notBefore) =>
        activity?.AddTag("peachtreebus.message.notbefore", notBefore.ToTagString());

    public static string ToTagString(this UtcDateTime value) => value.ToString("O");

    public static Activity? AddDestination(this Activity? activity, QueueName queueName) =>
        activity?.AddTag("messaging.destination.name", queueName.ToString());

    public static Activity? AddDestination(this Activity? activity, Topic topic) =>
        activity?.AddTag("messaging.destination.name", topic.ToString());

    public static Activity? AddQueueContext(this Activity? activity, QueueContext context) => activity
            ?.AddIncomingContext(context)
            ?.AddDestination(context.SourceQueue);

    public static Activity? AddSubscribedContext(this Activity? activity, SubscribedContext context) =>
        activity
            ?.AddIncomingContext(context)
            ?.AddDestination(context.Topic);

    public static Activity? AddIncomingContext(this Activity? activity, IIncomingContext context)
    {
        return activity
            ?.AddMessageId(context.MessageId)
            ?.AddPriority(context.MessagePriority)
            ?.AddMessageClass(context.MessageClass)
            ?.AddEnqueued(context.EnqueuedTime)
            ?.AddNotBefore(context.NotBefore);
        // todo add conversation id
        //?.AddTag("messaging.message.conversation_id", ????)
    }

    public static Activity? AddMessagingSystem(this Activity? activity) =>
        activity?.AddTag("messaging.system", "peachtreebus");

    public static Activity? AddMessagingOperation(this Activity? activity, string value) =>
        activity?.AddTag("messaging.operation.type", value);

    public static Activity? AddMessagingClientId(this Activity? activity) =>
        activity?.AddTag("messaging.client.id", Environment.MachineName);
}
