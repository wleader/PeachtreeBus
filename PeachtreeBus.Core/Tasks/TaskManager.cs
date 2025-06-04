using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Tasks;

public interface ITaskManager
{
    Task Run(CancellationToken token);
}

public class TaskManager(
    ITaskCounter availableTasks,
    IStarters starters,
    ISleeper sleeper)
    : ITaskManager
{
    private readonly ITaskCounter _availableTasks = availableTasks;
    private readonly IStarters _starters = starters;
    private readonly ISleeper _sleeper = sleeper;

    public async Task Run(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            // wait until there is at least one task slot available
            // before looking for something to do.
            await WaitForAvailableTasks(token);

            // these always get a chance to run.
            // that does mean though that if the subscribed and queued
            // use up the max MessageConcurrency, then currentTasks can be 
            // greater than the configuration MessageConcurrency.
            // but thats ok.
            await _starters.RunStarters(WhenTaskCompletes, token);

            if (_availableTasks.Available() > 0)
            {
                // the maintenace and messaging did not use up the
                // concurrency limit, Since there's not enough work
                // to do, sleep.
                await _sleeper.Sleep(1000);
            }
        }
    }

    private void WhenTaskCompletes(Task _)
    {
        _sleeper.Wake();
    }

    private async Task WaitForAvailableTasks(CancellationToken token)
    {
        while (_availableTasks.Available() < 1 && !token.IsCancellationRequested)
        {
            await _sleeper.Sleep(1000);
        }
    }
}
