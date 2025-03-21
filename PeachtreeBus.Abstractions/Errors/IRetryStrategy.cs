using System;

namespace PeachtreeBus.Errors;

/// <summary>
/// Defines a generic interface for retry strategies.
/// </summary>
/// <typeparam name="TContext"></typeparam>
public interface IRetryStrategy<TContext>
    where TContext : IContext
{
    RetryResult DetermineRetry(TContext context, Exception exception, FailureCount failureCount);
}
