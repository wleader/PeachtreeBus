using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Tasks;

public interface ICurrentTasks
{
    int Count { get; }

    void Add(Task task);

    Task WhenAny();
    Task WhenAll();
}

public class CurrentTasks : ICurrentTasks
{
    // when added, the task is added to the _started, and a continuation is added.
    // The continuation adds to _completed. The reason why is that code that reads
    // the count property likely wants to know how many tasks are running, but if
    // the continuation removes it, a very short task could complete before the
    // count property is read. If thats the case, the calling code might think
    // nothing was started. 
    // Instead, we only remove the completed from the started When the WhenAny
    // or WhenAll is called. This means that outside of this class, the count
    // won't going down without awaiting something, even if a task has already
    // completed.

    private readonly List<Task> _started = [];
    private readonly List<Task> _completed = [];
    private readonly SemaphoreSlim _semaphore = new(1);

    public int Count => _started.Count;

    private void WithSemaphore(Action action)
    {
        _semaphore.Wait();
        try
        {
            action();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Add(Task task) => WithSemaphore(() =>
    {
        _started.Add(task);
        task.ContinueWith(AddCompleted);
    });

    private void AddCompleted(Task task) => WithSemaphore(() =>
    {
        _completed.Add(task);
    });

    private void ReconcileCompleted() => WithSemaphore(() =>
    {
        _completed.ForEach(c => _started.Remove(c));
        _completed.Clear();
    });

    public Task WhenAny()
    {
        ReconcileCompleted();
        return _started.Count == 0
            ? Task.CompletedTask
            : Task.WhenAny(_started);
    }

    public Task WhenAll()
    {
        ReconcileCompleted();
        return _started.Count == 0
            ? Task.CompletedTask
            : Task.WhenAll(_started);
    }
}
