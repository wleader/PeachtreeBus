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

        using var scope = scopeFactory.Create();
        var startupTasks = scope.GetAllInstances<IRunOnStartup>();
        foreach (var startupTask in startupTasks)
        {
            startupTask.Run().GetAwaiter().GetResult();
        }
    }
}
