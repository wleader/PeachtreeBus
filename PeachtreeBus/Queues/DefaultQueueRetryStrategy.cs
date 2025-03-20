using PeachtreeBus.Errors;

namespace PeachtreeBus.Queues;

/// <summary>
/// A Default Retry Strategy for Queue Messages.
/// </summary>
public class DefaultQueueRetryStrategy : DefaultRetryStrategy<IQueueContext>, IQueueRetryStrategy { }
