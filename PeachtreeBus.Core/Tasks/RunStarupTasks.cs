using System.Collections.Generic;

namespace PeachtreeBus.Tasks;

public interface IRunStartupTasks
{
    void RunStartupTasks();
}

public class RunStarupTasks(
    IBusConfiguration busConfiguration,
    IScopeFactory scopeFactory)
    : IRunStartupTasks
{
    public void RunStartupTasks()
    {
        if (!busConfiguration.UseStartupTasks)
            return;

        using var accessor = scopeFactory.Create();
        var startupTasks = accessor.GetRequiredService<IEnumerable<IRunOnStartup>>();
        foreach (var startupTask in startupTasks)
        {
            startupTask.Run().GetAwaiter().GetResult();
        }
    }
}
