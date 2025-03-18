using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System;

namespace PeachtreeBus.Errors;

/// <summary>
/// Defines a generic default retry strategy.
/// </summary>
/// <typeparam name="TContext"></typeparam>
public abstract class DefaultRetryStrategy<TContext> : IRetryStrategy<TContext>
    where TContext : IContext
{
    public int MaxRetries { get; set; } = 5;

    public RetryResult DetermineRetry(TContext context, Exception exception, int failureCount)
    {
        return new(
            failureCount < MaxRetries,
            TimeSpan.FromSeconds(failureCount * 5));
    }
}

/// <summary>
/// A Default Retry Strategy for Queue Messages.
/// </summary>
public class DefaultQueueRetryStrategy : DefaultRetryStrategy<IQueueContext>, IQueueRetryStrategy { }

/// <summary>
/// A Default Retry Strategy for Subscribed Messages.
/// </summary>
public class DefaultSubscribedRetryStrategy : DefaultRetryStrategy<ISubscribedContext>, ISubscribedRetryStrategy { }
