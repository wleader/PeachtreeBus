using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System.Collections.Generic;

namespace PeachtreeBus.Tasks;

public interface IStarters
{
    public IEnumerable<IStarter> GetMaintenanceStarters();
    public IEnumerable<IStarter> GetMessagingStarters();
}
public class Starters(
    IUpdateSubscriptionsStarter updateSubscriptions,
    ICleanSubscriptionsStarter cleanSubscriptions,
    ICleanSubscribedPendingStarter cleanSubscribedPending,
    ICleanSubscribedCompletedStarter cleanSubscribedCompleted,
    ICleanSubscribedFailedStarter cleanSubscribedFailed,
    ICleanQueuedCompletedStarter cleanQueueCompleted,
    ICleanQueuedFailedStarter cleanQueueFailed,
    IProcessSubscribedStarter processSubscribed,
    IProcessQueuedStarter processQueued)
    : IStarters
{
    private readonly IStarter[] _maintenanceStarters =
    [
        updateSubscriptions,
        cleanSubscriptions,
        cleanSubscribedPending,
        cleanSubscribedCompleted,
        cleanSubscribedFailed,
        cleanQueueCompleted,
        cleanQueueFailed,
    ];

    private readonly IStarter[] _messageStarters =
    [
        processSubscribed,
        processQueued
    ];

    public IEnumerable<IStarter> GetMaintenanceStarters() => _maintenanceStarters;
    public IEnumerable<IStarter> GetMessagingStarters() => _messageStarters;
}
