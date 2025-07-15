using Microsoft.Extensions.Logging;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Tasks;

public interface IStarters
{
    public Task RunStarters(CancellationToken token);
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
    public async Task RunStarters(CancellationToken token)
    {
        await TryRunStarter(updateSubscriptions, token);
        await TryRunStarter(cleanSubscriptions, token);
        await TryRunStarter(cleanSubscribedPending, token);
        await TryRunStarter(cleanSubscribedCompleted, token);
        await TryRunStarter(cleanSubscribedFailed, token);
        await TryRunStarter(cleanQueueCompleted, token);
        await TryRunStarter(cleanQueueFailed, token);
        await TryRunStarter(processSubscribed, token);
        await TryRunStarter(processQueued, token);
    }

    private async Task TryRunStarter(IStarter starter, CancellationToken token)
    {
        try
        {
            await starter.Start(token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            log.StarterException(starter.GetType(), ex);
        }
    }
}
