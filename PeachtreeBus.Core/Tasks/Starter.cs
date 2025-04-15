﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Tasks;

public interface IStarter
{
    Task Start(Action<Task> continueWith, CancellationToken cancellationToken);
}

public abstract class Starter<TRunner>(
    IWrappedScopeFactory scopeFactory,
    ITracker tracker,
    ITaskCounter taskCounter)
    : IStarter
    where TRunner : class, IRunner
{
    private readonly IWrappedScopeFactory _scopeFactory = scopeFactory;
    private readonly ITracker _tracker = tracker;
    private readonly ITaskCounter _taskCounter = taskCounter;

    public async Task Start(Action<Task> continueWith, CancellationToken cancellationToken)
    {
        if (!_tracker.ShouldStart) return;
        var estimate = Math.Min(_taskCounter.Available(), await EstimateDemand());
        AddRunners(estimate, continueWith, cancellationToken);
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
        _taskCounter.Increment();
        _tracker.Start();
        Task.Run(() => runner.RunRepeatedly(cancellationToken)
            .ContinueWith((_) => WhenRunnerCompletes(scope), CancellationToken.None)
            .ContinueWith(continueWith, CancellationToken.None),
            CancellationToken.None);
    }

    private void WhenRunnerCompletes(IWrappedScope scope)
    {
        _taskCounter.Decrement();
        _tracker.WorkDone();
        scope.Dispose();
    }

    protected virtual Task<int> EstimateDemand() => Task.FromResult(1);
}
