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
    ICurrentTasks tasks,
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

    private static readonly TimeSpan idleDelay = TimeSpan.FromSeconds(1);

    public async Task Run(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            // get any newly started tasks.
            var startCount = await starters.RunStarters(token).ConfigureAwait(false);

            if (tasks.Count == 0 && startCount == 0)
                tasks.Add(delayFactory
                    .Delay(idleDelay, CancellationToken.None));

            await tasks.WhenAny(token).ConfigureAwait(false);
        }
        await tasks.WhenAll().ConfigureAwait(false);
    }
}
