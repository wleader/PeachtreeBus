using PeachtreeBus.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Tasks;

public interface ICurrentTasks
{
    int Count { get; }

    void Add(Task task);
    Task WhenAll();
}

public class CurrentTasks(
    IMeters meters)
    : ICurrentTasks
{
    private readonly List<Task> _tasks = [];
    private readonly SemaphoreSlim _semaphore = new(1);

    public int Count =>_semaphore.Invoke(() =>  _tasks.Count);

    public void Add(Task task) => _semaphore.Invoke(() =>
    {
        meters.StartTask();
        task.ContinueWith(TaskCompleted);
        _tasks.Add(task);
    });

    private void TaskCompleted(Task task) => _semaphore.Invoke(() =>
    {
        System.Diagnostics.Debug.Assert(_tasks.Contains(task));
        _tasks.Remove(task);
        meters.EndTask();
    });

    public async Task WhenAll()
    {
        while(_tasks.Any())
        {
            await Task.Delay(1);
        }
    }
}
