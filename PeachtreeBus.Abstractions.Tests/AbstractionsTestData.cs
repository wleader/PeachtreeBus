using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using PeachtreeBus.Subscriptions;
using System;
using System.Runtime.CompilerServices;

namespace PeachtreeBus.Abstractions.Tests;

public static class AbstractionsTestData
{
    public static readonly UniqueIdentity DefaultMessageId = new(Guid.Parse("36dcb8bb-8717-4307-927d-4947ee1ea1ad"));
    public static readonly SubscriberId DefaultSubscriberId = new(Guid.Parse("e8291248-c4fb-4b7e-ab7d-86df2bcea319"));
    public static readonly Topic DefaultTopic = new(nameof(DefaultTopic));
    public static readonly Topic DefaultTopic2 = new(nameof(DefaultTopic2));
    public static readonly QueueName DefaultQueueName = new(nameof(DefaultQueueName));
    public static readonly SagaName DefaultSagaName = new(nameof(DefaultSagaName));
    public static readonly UtcDateTime Now = new DateTime(2022, 2, 23, 10, 49, 32, 33, DateTimeKind.Utc);

    public static readonly UserHeaders DefaultUserHeaders = new()
    {
        { "Key1", "Value1" },
        { "Key2", "Value2" }
    };

    public static readonly SubscriberId UnintializedSubscriberId = (SubscriberId)RuntimeHelpers.GetUninitializedObject(typeof(SubscriberId));

}
