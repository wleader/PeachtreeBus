using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Tests.Sagas;
using System;
using System.Runtime.CompilerServices;

namespace PeachtreeBus.Tests;

public static class TestData
{
    public static readonly UniqueIdentity DefaultMessageId = new(Guid.Parse("36dcb8bb-8717-4307-927d-4947ee1ea1ad"));
    public static readonly SubscriberId DefaultSubscriberId = new(Guid.Parse("e8291248-c4fb-4b7e-ab7d-86df2bcea319"));
    public static readonly SerializedData DefaultHeaders = new("{}");
    public static readonly SerializedData DefaultBody = new("{}");
    public static readonly Identity DefaultId = new(1);
    public static readonly Category DefaultCategory = new(nameof(DefaultCategory));
    public static readonly QueueName DefaultQueueName = new("DefaultQueue");
    public static readonly SagaName DefaultSagaName = new("DefaultSagaName");

    public static readonly SubscriberId UnintializedSubscriberId = (SubscriberId)RuntimeHelpers.GetUninitializedObject(typeof(SubscriberId));

    public static QueueMessage CreateQueueMessage(
        Identity? id = null,
        UniqueIdentity? messageId = null,
        int priority = 0,
        UtcDateTime? notBefore = null,
        UtcDateTime? enqueued = null,
        SerializedData? headers = null,
        SerializedData? body = null)
    {
        return new()
        {
            Id = id ?? DefaultId,
            MessageId = messageId ?? DefaultMessageId,
            Priority = priority,
            NotBefore = notBefore ?? DateTime.UtcNow,
            Enqueued = enqueued ?? DateTime.UtcNow,
            Headers = headers ?? DefaultHeaders,
            Body = body ?? DefaultBody
        };
    }

    public static SubscribedMessage CreateSubscribedMessage(
        Identity? id = null,
        SubscriberId? subscriberId = null,
        UniqueIdentity? messageId = null,
        int priority = 0,
        UtcDateTime? notBefore = null,
        UtcDateTime? enqueued = null,
        UtcDateTime? validUntil = null,
        SerializedData? headers = null,
        SerializedData? body = null)
    {
        return new()
        {
            Id = id ?? DefaultId,
            SubscriberId = subscriberId ?? DefaultSubscriberId,
            MessageId = messageId ?? DefaultMessageId,
            Priority = priority,
            NotBefore = notBefore ?? DateTime.UtcNow,
            Enqueued = enqueued ?? DateTime.UtcNow,
            ValidUntil = validUntil ?? DateTime.UtcNow.AddMinutes(5),
            Headers = headers ?? DefaultHeaders,
            Body = body ?? DefaultBody
        };
    }

    public static TestSagaMessage1 CreateUserMessage()
    {
        return new();
    }

    public static Headers CreateHeaders(object? userMessage)
    {
        userMessage ??= CreateUserMessage();
        return new(userMessage.GetType());
    }

    public static Headers CreateHeadersWithUnrecognizedMessageClass()
    {
        const string UnrecognizedMessageClass = "PeachtreeBus.Tests.Sagas.NotARealMessageType, PeachtreeBus.Tests";
        Assert.IsNull(Type.GetType(UnrecognizedMessageClass), "A message class that was not supposed to exist, exists.");
        return new() { MessageClass = UnrecognizedMessageClass };
    }

    public static InternalQueueContext CreateQueueContext(
        object? userMessage = null,
        QueueMessage? messageData = null,
        QueueName? sourceQueue = null,
        Headers? headers = null)
    {
        userMessage ??= CreateUserMessage();
        messageData ??= CreateQueueMessage();
        headers ??= CreateHeaders(userMessage);

        return new InternalQueueContext()
        {
            Message = userMessage,
            MessageId = messageData.MessageId,
            MessageData = messageData,
            SourceQueue = sourceQueue ?? DefaultQueueName,
            Headers = headers,
        };
    }

    public static InternalSubscribedContext CreateSubscribedContext(
        object? userMessage = null,
        SubscribedMessage? messageData = null,
        Headers? headers = null)
    {
        userMessage ??= CreateUserMessage();
        messageData ??= CreateSubscribedMessage();
        headers ??= CreateHeaders(userMessage);
        return new()
        {
            Message = userMessage,
            MessageId = messageData.MessageId,
            MessageData = messageData,
            Headers = headers,
        };
    }
}
