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
    public Task<int> RunStarters(CancellationToken token);
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
    public async Task<int> RunStarters(CancellationToken token)
    {
        int result = 0;
        result += await RunStarter(updateSubscriptions, token);
        result += await RunStarter(cleanSubscriptions, token);
        result += await RunStarter(cleanSubscribedPending, token);
        result += await RunStarter(cleanSubscribedCompleted, token);
        result += await RunStarter(cleanSubscribedFailed, token);
        result += await RunStarter(cleanQueueCompleted, token);
        result += await RunStarter(cleanQueueFailed, token);
        result += await RunStarter(processSubscribed, token);
        result += await RunStarter(processQueued, token);
        return result;
    }

    private async Task<int> RunStarter(IStarter starter, CancellationToken token)
    {
        try
        {
            return await starter.Start(token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            log.StarterException(starter.GetType(), ex);
            return 0;
        }
    }
}
