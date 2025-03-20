using PeachtreeBus.Errors;

namespace PeachtreeBus.Queues;

/// <summary>
/// Defines a retry strategy for Queue Messages
/// </summary>
public interface IQueueRetryStrategy : IRetryStrategy<IQueueContext> { }