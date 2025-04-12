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
    IStarters starters)
    : ITaskManager
{
    private readonly IProvideShutdownSignal _shutdownSignal = shutdownSignal;
    private readonly IBusConfiguration _configuration = configuration;
    private readonly IStarters _starters = starters;

    private readonly object _sleepLock = new();
    private TaskCompletionSource _sleepTaskSource = new();
    private long _taskCount = 0;

    public async Task Run()
    {
        var shutdownToken = _shutdownSignal.GetCancellationToken();

        while (!shutdownToken.IsCancellationRequested)
        {
            // these always get a chance to run.
            // that does mean though that if the subscribed and queued
            // use up the max MessageConcurrency, then currentTasks can be 
            // greater than the configuration MessageConcurrency.
            // but thats ok.
            foreach (var starter in _starters.GetMaintenanceStarters())
            {
                var count = await starter.Start(1, WhenTaskCompletes, shutdownToken);
                Interlocked.Add(ref _taskCount, count);
            }

            var available = await WaitForAvailableTasks();
            foreach (var starter in _starters.GetMessagingStarters())
            {
                var count = await starter.Start(available, WhenTaskCompletes, shutdownToken);
                available -= count;
                Interlocked.Add(ref _taskCount, count);
            }

            if (available > 0)
            {
                await Sleep(1000);
            }
        }
    }

    private Task Sleep(int millisecondsDelay)
    {
        lock (_sleepLock)
        {
            _sleepTaskSource = new();
            Task.Delay(millisecondsDelay)
                .ContinueWith((_) => _sleepTaskSource.SetResult())
                .Start();
            return _sleepTaskSource.Task;
        }
    }

    private void Wakeup() { lock (_sleepLock) { _sleepTaskSource.SetCanceled(); } }

    private void WhenTaskCompletes(Task completed)
    {
        Interlocked.Decrement(ref _taskCount);
        Wakeup();
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

    private int AvailableTasks() => (int)(_configuration.MessageConcurrency - Interlocked.Read(ref _taskCount));
}
