using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Tasks;

public interface ITaskManager
{
    Task Run(CancellationToken token);
}

public class TaskManager(
    IDelayFactory delayFactory,
    IStarters starters)
    : ITaskManager
{
    // A little note on how this works.
    // There is a list of current tasks.
    // The starters gives back a list of new tasks.
    // When any started task completes, the loop can continue and look for more work.
    // (There is an assumption here that a task completing can cause more work.)
    // If the starters gives back no new tasks, then an idle delay is added.
    // this means that each time a task completes, it can look for more,
    // and when ther is no new tasks, it will sleep.
    // This allows new tasks to start as soon as there is capacity.

    private readonly object _lock = new();
    private readonly List<Task> _currentTasks = [];
    private static readonly TimeSpan idleDelay = TimeSpan.FromSeconds(1);

    public async Task Run(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            // get any newly started tasks.
            var newTasks = await starters.RunStarters(RemoveFromCurrentTasks, token).ConfigureAwait(false);

            if (_currentTasks.Count == 0 && newTasks.Count == 0)
                newTasks.Add(delayFactory
                    .Delay(idleDelay, CancellationToken.None)
                    .ContinueWith(RemoveFromCurrentTasks, CancellationToken.None));

            // keep track of all the incomplete tasks.
            lock (_lock)
            {
                foreach (var t in newTasks)
                {
                    _currentTasks.Add(t);
                }
            }

            await WaitForAnyCurrentTask(token).ConfigureAwait(false);
        }
        await Task.WhenAll(_currentTasks).ConfigureAwait(false);
    }

    private Task WaitForAnyCurrentTask(CancellationToken token)
    {
        if (token.IsCancellationRequested ||
            _currentTasks.Count == 0)
            return Task.CompletedTask;
        return Task.WhenAny(_currentTasks);
    }

    private void RemoveFromCurrentTasks(Task task)
    {
        lock (_lock) { _currentTasks.Remove(task); }
    }
}
