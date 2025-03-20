using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System;
using System.Collections.Generic;

namespace PeachtreeBus;

public abstract class BaseConfiguration
{
    public bool UseDefaultFailedHandler { get; init; } = true;
    public bool UseDefaultRetryStrategy { get; init; } = true;
    public bool CleanCompleted { get; init; } = true;
    public TimeSpan CleanCompleteAge { get; init; } = TimeSpan.FromDays(1);
    public bool CleanFailed { get; init; } = true;
    public TimeSpan CleanFailedAge { get; set; } = TimeSpan.FromDays(7);
    public TimeSpan CleanInterval { get; set; } = TimeSpan.FromMinutes(5);
    public int CleanMaxRows { get; set; } = 100;
}

public class QueueConfiguration : BaseConfiguration
{
    public required QueueName QueueName { get; init; }
}

public class SubscriptionConfiguration : BaseConfiguration
{
    public required SubscriberId SubscriberId { get; init; }
    public required IList<Topic> Topics { get; init; }
    public TimeSpan Lifespan { get; set; } = TimeSpan.FromHours(1);
}

public class PublishConfiguration
{
    public TimeSpan Lifespan { get; init; } = TimeSpan.FromDays(1);
}

public interface IBusConfiguration
{
    string ConnectionString { get; }
    SchemaName Schema { get; }
    QueueConfiguration? QueueConfiguration { get; init; }
    SubscriptionConfiguration? SubscriptionConfiguration { get; init; }
    PublishConfiguration PublishConfiguration { get; }
    bool UseDefaultSerialization { get; }
    bool UseStartupTasks { get; }
}

public class BusConfiguration : IBusConfiguration
{
    public required string ConnectionString { get; init; }
    public required SchemaName Schema { get; init; }
    public QueueConfiguration? QueueConfiguration { get; init; }
    public SubscriptionConfiguration? SubscriptionConfiguration { get; init; }
    public PublishConfiguration PublishConfiguration { get; init; } = new();
    public bool UseDefaultSerialization { get; init; } = true;
    public bool UseStartupTasks { get; init; } = true;
}
