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
    IScopeFactory scopeFactory,
    ITracker tracker,
    ITaskCounter taskCounter,
    IEstimator estimator)
    : IStarter
    where TRunner : class, IRunner
{
    public async Task<List<Task>> Start(Action<Task> continueWith, CancellationToken cancellationToken)
    {
        if (!tracker.ShouldStart) return [];

        var available = taskCounter.Available();
        // don't even bother estimating if there is no availability for more tasks.
        if (available < 1) return [];

        var estimate = Math.Min(available, await estimator.EstimateDemand());
        return AddRunners(estimate, continueWith, cancellationToken);
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
