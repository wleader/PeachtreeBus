namespace PeachtreeBus.SimpleInjector.Tests;

public class TestStartupTask : IRunOnStartup
{
    public Task Run()
    {
        return Task.CompletedTask;
    }
}
