using Microsoft.Extensions.Logging;
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
    ILogger<Starters> log,
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
        result.AddRange(await RunStarter(updateSubscriptions, continueWith, token));
        result.AddRange(await RunStarter(cleanSubscriptions, continueWith, token));
        result.AddRange(await RunStarter(cleanSubscribedPending, continueWith, token));
        result.AddRange(await RunStarter(cleanSubscribedCompleted, continueWith, token));
        result.AddRange(await RunStarter(cleanSubscribedFailed, continueWith, token));
        result.AddRange(await RunStarter(cleanQueueCompleted, continueWith, token));
        result.AddRange(await RunStarter(cleanQueueFailed, continueWith, token));
        result.AddRange(await RunStarter(processSubscribed, continueWith, token));
        result.AddRange(await RunStarter(processQueued, continueWith, token));
        return result;
    }

    private async Task<List<Task>> RunStarter(IStarter starter, Action<Task> continueWith, CancellationToken token)
    {
        try
        {
            return await starter.Start(continueWith, token);
        }
        catch (Exception ex)
        {
            log.StarterException(starter.GetType(), ex);
            return [];
        }
    }
}
