using PeachtreeBus.Errors;

namespace PeachtreeBus.Subscriptions;

/// <summary>
/// Defines a retry strategy for Subscribed Messages.
/// </summary>
public interface ISubscribedRetryStrategy : IRetryStrategy<ISubscribedContext> { }
