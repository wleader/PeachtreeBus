namespace PeachtreeBus.ScannableAssembly;

public abstract class RunOnStartup : IRunOnStartup
{
    public Task Run() => Task.CompletedTask;
}

public class RunOnStartup1 : RunOnStartup;
public class RunOnStartup2 : RunOnStartup;