using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Abstractions.Tests.TestClasses;
using PeachtreeBus.ClassNames;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using PeachtreeBus.Subscriptions;
using System;
using System.Runtime.CompilerServices;

namespace PeachtreeBus.Core.Tests;

public static class TestData
{
    public static readonly UniqueIdentity DefaultMessageId = new(Guid.Parse("36dcb8bb-8717-4307-927d-4947ee1ea1ad"));
    public static readonly SubscriberId DefaultSubscriberId = new(Guid.Parse("e8291248-c4fb-4b7e-ab7d-86df2bcea319"));
    public static readonly SerializedData DefaultHeaders = new("{}");
    public static readonly SerializedData DefaultBody = new("{}");
    public static readonly SerializedData DefaultSagaData = new("{}");
    public static readonly Identity DefaultId = new(1);
    public static readonly Topic DefaultTopic = new(nameof(DefaultTopic));
    public static readonly Topic DefaultTopic2 = new(nameof(DefaultTopic2));
    public static readonly QueueName DefaultQueueName = new(nameof(DefaultQueueName));
    public static readonly SagaName DefaultSagaName = new(nameof(DefaultSagaName));
    public static readonly SagaKey DefaultSagaKey = new(nameof(DefaultSagaKey));
    public static readonly UtcDateTime Now = new DateTime(2022, 2, 23, 10, 49, 32, 33, DateTimeKind.Utc);

    public static readonly UserHeaders DefaultUserHeaders = new()
    {
        { "Key1", "Value1" },
        { "Key2", "Value2" }
    };

    public static readonly SubscriberId UnintializedSubscriberId = (SubscriberId)RuntimeHelpers.GetUninitializedObject(typeof(SubscriberId));

    public static QueueData CreateQueueData(
        Identity? id = null,
        UniqueIdentity? messageId = null,
        int priority = 0,
        UtcDateTime? notBefore = null,
        UtcDateTime? enqueued = null,
        Headers? headers = null,
        SerializedData? body = null)
    {
        return new()
        {
            Id = id ?? DefaultId,
            MessageId = messageId ?? DefaultMessageId,
            Priority = priority,
            NotBefore = notBefore ?? DateTime.UtcNow,
            Enqueued = enqueued ?? DateTime.UtcNow,
            Headers = headers ?? new() { MessageClass = typeof(TestQueuedMessage).GetClassName() },
            Body = body ?? DefaultBody
        };
    }

    public static SubscribedData CreateSubscribedData(
        Identity? id = null,
        SubscriberId? subscriberId = null,
        UniqueIdentity? messageId = null,
        int priority = 0,
        UtcDateTime? notBefore = null,
        UtcDateTime? enqueued = null,
        UtcDateTime? validUntil = null,
        Headers? headers = null,
        SerializedData? body = null,
        Topic? topic = null)
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
            Headers = headers ?? new() { MessageClass = ClassName.Default },
            Body = body ?? DefaultBody,
            Topic = topic ?? DefaultTopic,
        };
    }

    public static SagaData CreateSagaData(
        UniqueIdentity? sagaId = null,
        Identity? id = null,
        bool blocked = false,
        SagaKey? sagaKey = null,
        SagaMetaData? metaData = null,
        SerializedData? data = null)
    {
        return new()
        {
            SagaId = sagaId ?? UniqueIdentity.New(),
            Blocked = blocked,
            Id = id ?? DefaultId,
            Key = sagaKey ?? DefaultSagaKey,
            MetaData = metaData ?? new(),
            Data = data ?? DefaultSagaData
        };
    }

    public static TestQueuedMessage CreateQueueUserMessage() => new();
    public static TestSubscribedMessage CreateSubscribedUserMessage() => new();

    public static Headers CreateHeaders(object? userMessage)
    {
        userMessage ??= CreateQueueUserMessage();
        return new()
        {
            MessageClass = userMessage.GetType().GetClassName()
        };
    }

    public static Headers CreateHeadersWithUnrecognizedMessageClass()
    {
        const string UnrecognizedMessageClass = "PeachtreeBus.Tests.Sagas.NotARealMessageType, PeachtreeBus.Tests";
        Assert.IsNull(Type.GetType(UnrecognizedMessageClass), "A message class that was not supposed to exist, exists.");
        return new()
        {
            MessageClass = new(UnrecognizedMessageClass)
        };
    }

    public static QueueContext CreateQueueContext(
        Func<object>? userMessageFunc = null,
        QueueData? messageData = null,
        QueueName? sourceQueue = null,
        SagaData? sagaData = null)
    {
        var messageObject = userMessageFunc?.Invoke() ?? CreateQueueUserMessage();
        messageData ??= CreateQueueData();
        sagaData ??= CreateSagaData();

        return new()
        {
            Message = messageObject,
            Data = messageData,
            SourceQueue = sourceQueue ?? DefaultQueueName,
            SagaData = sagaData,
        };
    }

    public static SubscribedContext CreateSubscribedContext(
        object? userMessage = null,
        SubscribedData? messageData = null,
        Headers? headers = null)
    {
        userMessage ??= CreateQueueUserMessage();
        messageData ??= CreateSubscribedData(
            headers: headers ?? CreateHeaders(userMessage));
        return new()
        {
            Message = userMessage,
            Data = messageData,
        };
    }

    public static QueueConfiguration CreateQueueConfiguration()
    {
        return new()
        {
            QueueName = DefaultQueueName
        };
    }

    public static SubscriptionConfiguration CreateSubscriptionConfiguration()
    {
        return new()
        {
            SubscriberId = DefaultSubscriberId,
            Topics = [DefaultTopic, DefaultTopic2]
        };
    }

    public static BusConfiguration CreateBusConfiguration()
    {
        return new()
        {
            ConnectionString = "Server=(local);Database=db",
            Schema = new("PeachtreeBus"),
            QueueConfiguration = CreateQueueConfiguration(),
            SubscriptionConfiguration = CreateSubscriptionConfiguration(),
        };
    }

    public static SendContext CreateSendContext(
        object? userMessage = null,
        QueueName? destination = null,
        UtcDateTime? notBefore = null)
    {
        userMessage ??= CreateQueueUserMessage();
        return new SendContext()
        {
            Message = userMessage,
            Destination = destination ?? DefaultQueueName,
            Data = new()
            {
                Headers = new()
                {
                    MessageClass = userMessage.GetType().GetClassName(),
                    UserHeaders = [],
                },
                Body = default,
                Enqueued = TestData.Now,
                NotBefore = notBefore ?? TestData.Now,
                MessageId = default,
                Priority = 0,
            },
        };
    }

    public static PublishContext CreatePublishContext(
        object? userMessage = null,
        Topic? topic = null)
    {
        userMessage ??= CreateSubscribedUserMessage();
        return new PublishContext()
        {
            Message = userMessage,
            Data = new()
            {
                Headers = new()
                {
                    MessageClass = userMessage.GetType().GetClassName(),
                },
                Body = default,
                Enqueued = TestData.Now,
                NotBefore = TestData.Now,
                MessageId = default,
                Priority = 0,
                ValidUntil = TestData.Now.AddDays(1),
                Topic = topic ?? DefaultTopic,
            },
        };
    }

    public static SagaMetaData CreateSagaMetaData(
        DateTime? started = null,
        DateTime? lastMessageTime = null)
    {
        return new()
        {
            Started = started ?? DateTime.UtcNow,
            LastMessageTime = lastMessageTime ?? DateTime.UtcNow,
        };

    }
}
