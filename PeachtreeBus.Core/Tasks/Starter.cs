using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Tasks;

public interface IStarter
{
    Task Start(CancellationToken cancellationToken);
}

public abstract class Starter<TRunner>(
    ILogger<Starter<TRunner>> log,
    IScopeFactory scopeFactory,
    ICurrentTasks currentTasks,
    ITracker tracker,
    ITaskCounter taskCounter,
    IEstimator estimator,
    IBusDataAccess dataAccess)
    : IStarter
    where TRunner : class, IRunner
{
    public async Task Start(CancellationToken cancellationToken)
    {
        var count = await DetermineRunnerCount().ConfigureAwait(false);
        for (int i = 0; i < count; i++)
        {
            AddRunner(cancellationToken);
        }
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
            return Math.Min(available, await estimator.EstimateDemand().ConfigureAwait(false));
        }
        catch
        (Exception ex)
        {
            log.StarterException(this.GetType(), ex);
            return 0;
        }
    }

    private void AddRunner(CancellationToken cancellationToken)
    {
        var accessor = scopeFactory.Create();
        var runner = accessor.GetRequiredService<TRunner>();
        taskCounter.Increment();
        tracker.Start();
        currentTasks.Add(Task.Run(
                () => runner.RunRepeatedly(cancellationToken)
                            .ContinueWith((_) => WhenRunnerCompletes(accessor), CancellationToken.None)
                            .ConfigureAwait(false),
                CancellationToken.None));
    }

    private void WhenRunnerCompletes(IServiceProviderAccessor accessor)
    {
        taskCounter.Decrement();
        tracker.WorkDone();
        accessor.Dispose();
    }
}
