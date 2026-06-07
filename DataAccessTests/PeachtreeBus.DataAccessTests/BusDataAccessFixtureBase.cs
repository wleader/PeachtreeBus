using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseTesting;

namespace PeachtreeBus.DataAccessTests;

public abstract class BusDataAccessFixtureBase
{
    protected ITestDataAccess TestDataAccess { get; private set; } = null!;
    protected IBusDataAccess BusDataAccess { get; private set; } = null!;
    protected ITestConfig TestConfig { get; private set; } = null!;
    private IServiceScope Scope { get; set; } = null!;

    public virtual void Initialize()
    {
        Scope = TestServices.ServiceProvider.CreateScope();

        TestConfig = Scope.ServiceProvider.GetRequiredService<ITestConfig>();
        BusDataAccess = Scope.ServiceProvider.GetRequiredService<IBusDataAccess>();
        BusDataAccess.Reconnect();
        TestDataAccess = Scope.ServiceProvider.GetRequiredService<ITestDataAccess>();
        TestDataAccess.Initialize();
        TestDataAccess.CleanEverything();
    }

    public virtual void Cleanup()
    {
        TestDataAccess.CleanEverything();
        TestDataAccess.CloseConnections();
        Scope.Dispose();
    }

    protected void Repeat(Action action, int count)
    {
        for (var i = 0; i < count; i++)
        {
            action();
        }
    }

    protected async Task Repeat(Func<Task> action, int count)
    {
        for (var i = 0; i < count; i++)
        {
            await action();
        }
    }
}