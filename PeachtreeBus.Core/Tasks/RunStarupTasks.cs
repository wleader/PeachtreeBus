using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeachtreeBus.Tasks;

public interface IRunStartupTasks
{
    void RunStartupTasks();
}

public class RunStarupTasks(
    IBusConfiguration busConfiguration,
    IWrappedScopeFactory scopeFactory)
    : IRunStartupTasks
{
    public void RunStartupTasks()
    {
        if (!busConfiguration.UseStartupTasks)
            return;

        List<Task> tasks = [];
        List<IWrappedScope> scopes = [];

        var startupTaskTypes = scopeFactory.GetImplementations<IRunOnStartup>();

        foreach (var t in startupTaskTypes)
        {
            var scope = scopeFactory.Create();
            scopes.Add(scope);
            var startupTask = (IRunOnStartup)scope.GetInstance(t);
            tasks.Add(startupTask.Run());
        }

        Task.WaitAll([.. tasks]);

        foreach (var s in scopes)
        { s.Dispose(); }
    }
}
