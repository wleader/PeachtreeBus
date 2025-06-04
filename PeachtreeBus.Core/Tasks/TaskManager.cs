using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Tasks;

public interface ITaskManager
{
    Task Run(CancellationToken token);
}

public class TaskManager(
    IStarters starters)
    : ITaskManager
{
    // A little note on how this works.
    // There is a list of current tasks.
    // It starts with a Delay task of 1 second.
    // The starters gives back a list of new tasks.
    // All the started tasks and delay task have a continuation that removes the completed task from the list.
    // The delay task's continuation adds a new delay task.
    // The while loop Adds new tasks, then waits for any of the tasks to complete.
    // The presense of the delay task in the list means that the loop is going to run at least once per second.
    // The loop could run sooner than that 1 second delay when a non-delay task compeltes.
    // This allows new tasks to start as soon as there is capacity.
    // Since it will always run at least once per second, regularly scheduled tasks like cleanup will
    // always get a chance to run, even if the queue processing tasks stay busy continuously.

    private readonly object _lock = new();
    private readonly List<Task> _currentTasks = [];

    public async Task Run(CancellationToken token)
    {
        AddDelayToCurrentTasks();

        while (!token.IsCancellationRequested)
        {
            // get any newly started tasks.
            var newTasks = await starters.RunStarters(RemoveFromCurrentTasks, token).ConfigureAwait(false);

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

    private void AddDelayToCurrentTasks()
    {
        var newInterval = Task.Delay(1000, CancellationToken.None)
            .ContinueWith(WhenDelayCompletes);
        lock (_lock) { _currentTasks.Add(newInterval); }
    }

    private void WhenDelayCompletes(Task task)
    {
        RemoveFromCurrentTasks(task);
        AddDelayToCurrentTasks();
    }

    private void RemoveFromCurrentTasks(Task task)
    {
        lock (_lock) { _currentTasks.Remove(task); }
    }
}
