using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System;

namespace PeachtreeBus.Errors;

/// <summary>
/// Used to communicate back to the reader classes what to do with
/// a message after an unhandled exception in a message handler.
/// </summary>
public readonly record struct RetryResult
{
    /// <summary>
    /// If True, the message will be retried.
    /// </summary>
    public bool ShouldRetry { get; }

    /// <summary>
    /// When ShouldRetry is true, then this valuecontrols how 
    /// long to wait before retrying the message.
    /// </summary>
    public TimeSpan Delay { get; }

    public RetryResult(bool shouldRetry, TimeSpan delay)
    {
        ShouldRetry = shouldRetry;
        Delay = delay;
    }
}

/// <summary>
/// Defines a generic interface for retry strategies.
/// </summary>
/// <typeparam name="TContext"></typeparam>
public interface IRetryStrategy<TContext>
    where TContext : IContext
{
    RetryResult DetermineRetry(TContext context, Exception exception, int failureCount);
}

/// <summary>
/// Defines a retry strategy for Subscribed Messages.
/// </summary>
public interface ISubscribedRetryStrategy : IRetryStrategy<ISubscribedContext> { }

/// <summary>
/// Defines a retry strategy for Queue Messages
/// </summary>
public interface IQueueRetryStrategy : IRetryStrategy<IQueueContext> { }