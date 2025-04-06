using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Tasks;

public interface ITaskManager
{
    Task Run();
}

public class TaskManager(
    IProvideShutdownSignal shutdownSignal,
    IBusConfiguration configuration,
    IBusDataAccess dataAccess,
    IWrappedScopeFactory scopeFactory,
    ISubscriptionUpdateTracker subscriptionUpdateTracker,
    ICleanQueuedTracker cleanQueuedTracker,
    ICleanSubscribedTracker cleanSubscribedTracker,
    ICleanSubscriptionsTracker cleanSubscriptionsTracker)
    : ITaskManager
{
    private readonly IProvideShutdownSignal _shutdownSignal = shutdownSignal;
    private readonly IBusConfiguration _configuration = configuration;
    private readonly IBusDataAccess _dataAccess = dataAccess;
    private readonly IWrappedScopeFactory _scopeFactory = scopeFactory;
    private readonly ISubscriptionUpdateTracker _subscriptionUpdateTracker = subscriptionUpdateTracker;
    private readonly ICleanQueuedTracker _cleanQueuedTracker = cleanQueuedTracker;
    private readonly INextRunTracker _cleanSubscribedTracker = cleanSubscribedTracker;
    private readonly INextRunTracker _cleanSubscriptionsTracker = cleanSubscriptionsTracker;
    private CancellationTokenSource sleepTokenSource = new();
    private long currentTasks = 0;

    public async Task Run()
    {
        var shutdownToken = _shutdownSignal.GetCancellationToken();

        do
        {
            // these always get a chance to run.
            // that does mean though that if the subscribed and queued
            // use up the max tasks, then currentTasks can be 
            // greater than the configuration MessageConcurrency.
            // but thats ok.
            UpdateSubscriptions();
            CleanSubscribed();
            CleanSubscriptions();
            CleanQueued();

            var available = await WaitForAvailableTasks();
            available = await AddSubscribedTasks(available);
            available = await AddQueuedTasks(available);

            if (available > 0)
            {
                await Sleep(3000);
            }
        }
        while (!shutdownToken.IsCancellationRequested);
    }

    private async Task Sleep(int millisecondsDelay)
    {
        try
        {
            CancellationToken token;
            lock (sleepTokenSource)
            {
                token = sleepTokenSource.Token;
            }
            await Task.Delay(millisecondsDelay, token);
        }
        catch (TaskCanceledException)
        {
            sleepTokenSource = new();
        }
    }

    private void Wakeup()
    {
        // if the task manager was sleeping,
        // wake it up so it can start another task.
        lock (sleepTokenSource)
        {
            sleepTokenSource.Cancel();
        }
    }

    private void CleanSubscriptions() =>
        AddIfDue<ICleanSubscriptionsTask>(_cleanSubscriptionsTracker);

    private void CleanSubscribed() =>
        AddIfDue<ICleanSubscribedTask>(_cleanSubscribedTracker);

    private void CleanQueued() =>
        AddIfDue<ICleanQueuedTask>(_cleanQueuedTracker);

    private void UpdateSubscriptions() =>
        AddIfDue<ISubscriptionUpdateTask>(_subscriptionUpdateTracker);

    private void AddIfDue<TTask>(INextRunTracker tracker) where TTask : class, IBaseTask
    {
        if (tracker.WorkDue) AddTask<TTask>();
    }

    private async Task<int> AddSubscribedTasks(int available)
    {
        if (_configuration.SubscriptionConfiguration is null) return available;
        var estimate = (int)await _dataAccess.EstimateSubscribedPending(_configuration.SubscriptionConfiguration!.SubscriberId);
        estimate = Math.Min(estimate, available);
        AddTasks<IProcessSubscribedTask>(estimate);
        return available - estimate;
    }

    private async Task<int> AddQueuedTasks(int available)
    {
        if (_configuration.QueueConfiguration is null) return available;
        var estimate = (int)await _dataAccess.EstimateQueuePending(_configuration.QueueConfiguration.QueueName);
        estimate = Math.Min(estimate, available);
        AddTasks<IProcessQueuedTask>(estimate);
        return available - estimate;
    }

    private void AddTasks<TTask>(int count) where TTask : class, IBaseTask
    {
        for (int i = 0; i < count; i++)
        {
            AddTask<TTask>();
        }
    }

    private void AddTask<TTask>() where TTask : class, IBaseTask
    {
        // fun fact. Interlock.Increment will overflow without an exception
        // which in this case is perfectly fine.
        Interlocked.Increment(ref currentTasks);
        // Todo, add an Internal Meter
        var scope = _scopeFactory.Create();
        var task = scope.GetInstance<TTask>();
        var t = task.Run(_shutdownSignal.GetCancellationToken());
        t.ContinueWith((t) => WhenTaskCompletes(t, scope));
    }

    private Task WhenTaskCompletes(Task completedTask, IWrappedScope scope)
    {
        scope.Dispose();
        Interlocked.Decrement(ref currentTasks);
        Wakeup();
        return completedTask;
    }

    private async Task<int> WaitForAvailableTasks()
    {
        var result = AvailableTasks();
        while (result < 1)
        {
            await Sleep(1000);
            result = AvailableTasks();
        }
        return result;
    }

    private int AvailableTasks() => (int)(_configuration.MessageConcurrency - Interlocked.Read(ref currentTasks));
}
