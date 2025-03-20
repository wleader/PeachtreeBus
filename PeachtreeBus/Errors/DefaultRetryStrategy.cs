using System;

namespace PeachtreeBus.Errors;

/// <summary>
/// Defines a generic default retry strategy.
/// </summary>
/// <typeparam name="TContext"></typeparam>
public abstract class DefaultRetryStrategy<TContext> : IRetryStrategy<TContext>
    where TContext : IIncomingContext
{
    public int MaxRetries { get; set; } = 5;

    public RetryResult DetermineRetry(TContext context, Exception exception, int failureCount)
    {
        return new(
            failureCount < MaxRetries,
            TimeSpan.FromSeconds(failureCount * 5));
    }
}
