using PeachtreeBus.Errors;

namespace PeachtreeBus.Subscriptions;

/// <summary>
/// A Default Retry Strategy for Subscribed Messages.
/// </summary>
public class DefaultSubscribedRetryStrategy : DefaultRetryStrategy<ISubscribedContext>, ISubscribedRetryStrategy { }
