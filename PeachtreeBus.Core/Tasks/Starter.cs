using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Tasks;

public interface IStarter
{
    Task<List<Task>> Start(Action<Task> continueWith, CancellationToken cancellationToken);
}

public abstract class Starter<TRunner>(
    ILogger<Starter<TRunner>> log,
    IScopeFactory scopeFactory,
    ITracker tracker,
    ITaskCounter taskCounter,
    IEstimator estimator,
    IBusDataAccess dataAccess)
    : IStarter
    where TRunner : class, IRunner
{
    public async Task<List<Task>> Start(Action<Task> continueWith, CancellationToken cancellationToken)
    {
        var count = await DetermineRunnerCount();
        return AddRunners(count, continueWith, cancellationToken);
    }

    private async Task<int> DetermineRunnerCount()
    {
        try
        {
            dataAccess.Reconnect();
            if (!tracker.ShouldStart) return 0;
            var available = taskCounter.Available();
            // don't even bother estimating if there is no availability for more tasks.
            if (available < 1) return 0;
            return Math.Min(available, await estimator.EstimateDemand());
        }
        catch
        (Exception ex)
        {
            log.StarterException(this.GetType(), ex);
            return 0;
        }
    }

    private List<Task> AddRunners(int count, Action<Task> continueWith, CancellationToken cancellationToken)
    {
        List<Task> result = new(count);
        for (int i = 0; i < count; i++)
        {
            result.Add(AddRunner(continueWith, cancellationToken));
        }
        return result;
    }

    private Task AddRunner(Action<Task> continueWith, CancellationToken cancellationToken)
    {
        var accessor = scopeFactory.Create();
        var runner = accessor.GetRequiredService<TRunner>();
        taskCounter.Increment();
        tracker.Start();
        return Task.Run(() => runner.RunRepeatedly(cancellationToken)
            .ContinueWith((_) => WhenRunnerCompletes(accessor), CancellationToken.None)
            .ContinueWith(continueWith, CancellationToken.None),
            CancellationToken.None);
    }

    private void WhenRunnerCompletes(IServiceProviderAccessor accessor)
    {
        taskCounter.Decrement();
        tracker.WorkDone();
        accessor.Dispose();
    }
}
