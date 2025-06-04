using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Tasks;

public interface IStarters
{
    public Task<List<Task>> RunStarters(Action<Task> continueWith, CancellationToken token);
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
    public async Task<List<Task>> RunStarters(Action<Task> continueWith, CancellationToken token)
    {
        var result = new List<Task>();
        result.AddRange(await updateSubscriptions.Start(continueWith, token));
        result.AddRange(await cleanSubscriptions.Start(continueWith, token));
        result.AddRange(await cleanSubscribedPending.Start(continueWith, token));
        result.AddRange(await cleanSubscribedCompleted.Start(continueWith, token));
        result.AddRange(await cleanSubscribedFailed.Start(continueWith, token));
        result.AddRange(await cleanQueueCompleted.Start(continueWith, token));
        result.AddRange(await cleanQueueFailed.Start(continueWith, token));
        result.AddRange(await processSubscribed.Start(continueWith, token));
        result.AddRange(await processQueued.Start(continueWith, token));
        return result;
    }
}
