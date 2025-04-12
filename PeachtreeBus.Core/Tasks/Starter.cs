using System;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Tasks;

public interface IStarter
{
    Task<int> Start(int available, Action<Task> continueWith, CancellationToken cancellationToken);
}

public abstract class Starter<TRunner>(
    IWrappedScopeFactory scopeFactory,
    ITracker tracker)
    : IStarter
    where TRunner : class, IRunner
{
    private readonly IWrappedScopeFactory _scopeFactory = scopeFactory;
    private readonly ITracker _tracker = tracker;

    public async Task<int> Start(int available, Action<Task> continueWith, CancellationToken cancellationToken)
    {
        if (!_tracker.ShouldStart) return 0;
        var estimate = Math.Min(available, await EstimateDemand());
        AddRunners(estimate, continueWith, cancellationToken);
        return estimate;
    }

    private void AddRunners(int count, Action<Task> continueWith, CancellationToken cancellationToken)
    {
        for (int i = 0; i < count; i++)
        {
            AddRunner(continueWith, cancellationToken);
        }
    }

    private void AddRunner(Action<Task> continueWith, CancellationToken cancellationToken)
    {
        var scope = _scopeFactory.Create();
        var runner = scope.GetInstance<TRunner>();
        _tracker.Start();
        runner.RunRepeatedly(cancellationToken)
            .ContinueWith((_) => WhenRunnerCompletes(scope), CancellationToken.None)
            .ContinueWith(continueWith, CancellationToken.None);
    }

    private void WhenRunnerCompletes(IWrappedScope scope)
    {
        _tracker.WorkDone();
        scope.Dispose();
    }

    protected virtual Task<int> EstimateDemand() => Task.FromResult(1);
}
