using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Errors;

/// <summary>
/// Defines a retry strategy for Subscribed Messages.
/// </summary>
public interface ISubscribedRetryStrategy : IRetryStrategy<ISubscribedContext> { }
