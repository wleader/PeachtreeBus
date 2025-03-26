using Microsoft.IdentityModel.Tokens;
using PeachtreeBus.Data;
using PeachtreeBus.Pipelines;
using PeachtreeBus.Queues;
using System;
using System.Diagnostics;
using System.Reflection;

namespace PeachtreeBus.Telemetry;

public interface IActivityFactory
{
    Activity? Receive(QueueContext context, DateTime started);
    Activity? PipelineStep<TContext>(IPipelineStep<TContext> step);
    Activity? Handler(IIncomingContext context, object handler);
    Activity? Send(ISendContext context);
}

public class ActivityFactory : IActivityFactory
{
    // Core messaging activities.
    public static readonly ActivitySource Messaging = new("PeachtreeBus.Messaging", "0.11.0");

    // calls into user code.
    public static readonly ActivitySource User = new("PeachtreeBus.User", "0.11.0");

    // Stuff that peachtree bus developers could be interested in, but most users wouldn't
    public static readonly ActivitySource Internal = new("PeachtreeBus.Internal", "0.11.0");

    public Activity? Receive(QueueContext context, DateTime started)
    {
        return Messaging.StartActivity(
            "receive " + context.SourceQueue.ToString(),
            ActivityKind.Client,
            null, // parent context
            startTime: started)
            ?.AddTag("messaging.system", "peachtreebus")
            ?.AddTag("messaging.operation.type", "receive")
            ?.AddTag("messaging.client.id", Environment.MachineName)
            ?.AddQueueContext(context);
    }

    public Activity? PipelineStep<TContext>(IPipelineStep<TContext>? step)
    {
        // we don't want to trace the final step, that's just part of the overhead from the user's
        // perspective, and clutters the trace view.
        var type = step?.GetType();
        if (DoNotTrace(type)) return null;

        return User.StartActivity(
            "peachtreebus.pipeline " + type!.Name,
            ActivityKind.Internal)
            ?.AddTag("peachtreebus.pipeline.type", type.FullName);
    }

    public Activity? Handler(IIncomingContext context, object handler)
    {
        var type = handler?.GetType();
        if (type is null) return null;

        return User.StartActivity(
            "peachtreebus.handler " + type.Name,
            ActivityKind.Internal)
            ?.AddTag("peachtreebus.handler.type", type.FullName)
            ?.AddIncomingContext(context);
    }

    public Activity? Send(ISendContext context)
    {
        return Messaging.StartActivity(
            "send " + context.Destination.ToString(),
            ActivityKind.Producer)
            ?.AddOutgoingContext(context)
            ?.AddDestination(context.Destination);
    }

    private static bool DoNotTrace(Type? type)
    {
        if (type is null) return true; // can't trace what doesn't exist.
        var baseType = type?.BaseType;
        return
            baseType is not null &&
            baseType.IsGenericType &&
            baseType.GetGenericTypeDefinition() == typeof(PipelineFinalStep<,>);
    }
}

public static class ActivityExtensions
{
    public static Activity? AddException(this Activity? activity, Exception? ex)
    {
        if (ex is null) return activity;
        return activity?.AddTag("error.type", ex.GetType().FullName);
    }

    public static Activity? AddQueueContext(this Activity? activity, QueueContext? context)
    {
        if (context is null) return activity;
        return activity?.AddIncomingContext(context)
            ?.AddDestination(context.SourceQueue);
    }

    public static Activity? AddIncomingContext(this Activity? activity, IIncomingContext? context)
    {
        if (context is null) return activity;
        return activity
            ?.AddMessageId(context.MessageId)
            ?.AddPriority(context.MessagePriority)
            ?.AddMessageClass(context.MessageClass)
            ?.AddEnqueued(context.EnqueuedTime)
            ?.AddNotBefore(context.NotBefore);
            // todo add conversation id
            //?.AddTag("messaging.message.conversation_id", ????)
    }

    public static Activity? AddOutgoingContext(this Activity? activity, IOutgoingContext? context)
    {
        if (context is null) return activity;
        return activity?.AddPriority(context.MessagePriority)
            ?.AddNotBefore(context.NotBefore);
    }

    public static Activity? AddDestination(this Activity? activity, QueueName queueName)
    {
        return activity?.AddTag("messaging.destination.name", queueName.ToString());
    }

    public static Activity? AddMessageId(this Activity? activity, UniqueIdentity id)
    {
        return activity?.AddTag("messaging.message.id", id.ToString());
    }

    public static Activity? AddPriority(this Activity? activity, int priority)
    {
        return activity?.AddTag("peachtreebus.message.priority", priority.ToString());
    }

    public static Activity? AddEnqueued(this Activity? activity, UtcDateTime enqueued)
    {
        return activity?.AddTag("peachtreebus.message.enqueued", enqueued.ToTagString());
    }

    public static Activity? AddMessageClass(this Activity? activity, string messageClass)
    {
        return activity?.AddTag("peachtreebus.message.class", messageClass);
    }

    public static Activity? AddNotBefore(this Activity? activity, UtcDateTime? notBefore)
    {
        if (!notBefore.HasValue) return activity;
        return activity?.AddTag("peachtreebus.message.notbefore", notBefore.Value.ToTagString());
    }

    public static string? ToTagString(this UtcDateTime value) => value.Value.ToString("O");
}
