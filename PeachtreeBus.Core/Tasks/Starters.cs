using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Tasks;

public interface IStarters
{
    public Task RunStarters(Action<Task> continueWith, CancellationToken token);
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
    public async Task RunStarters(Action<Task> continueWith, CancellationToken token)
    {
        await updateSubscriptions.Start(continueWith, token);
        await cleanSubscriptions.Start(continueWith, token);
        await cleanSubscribedPending.Start(continueWith, token);
        await cleanSubscribedCompleted.Start(continueWith, token);
        await cleanSubscribedFailed.Start(continueWith, token);
        await cleanQueueCompleted.Start(continueWith, token);
        await cleanQueueFailed.Start(continueWith, token);
        await processSubscribed.Start(continueWith, token);
        await processQueued.Start(continueWith, token);
    }
}
