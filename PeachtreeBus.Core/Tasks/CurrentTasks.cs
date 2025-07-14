using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Tasks;

public interface ICurrentTasks
{
    int Count { get; }

    void Add(Task task);

    Task WhenAny(CancellationToken token);
    Task WhenAll();
}

public class CurrentTasks : ICurrentTasks
{
    private readonly List<Task> _tasks = [];
    private readonly List<Task> _completed = [];
    private readonly SemaphoreSlim _semaphore = new(1);

    public int Count => _tasks.Count;

    public void Add(Task task)
    {
        _semaphore.Wait();
        try
        {
            _tasks.Add(task);
            task.ContinueWith(AddCompleted);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void AddCompleted(Task task)
    {
        _semaphore.Wait();
        try
        {
            _completed.Add(task);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void ReconcileCompleted()
    {
        _semaphore.Wait();
        try
        {
            _completed.ForEach(c => _tasks.Remove(c));
            _completed.Clear();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public Task WhenAny(CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Task.CompletedTask;

        ReconcileCompleted();

        if (_tasks.Count == 0)
            return Task.CompletedTask;

        return Task.WhenAny(_tasks);
    }

    public Task WhenAll()
    {
        ReconcileCompleted();

        return Task.WhenAll(_tasks);
    }
}
