using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PeachtreeBus.DatabaseTesting;

namespace PeachtreeBus.DataAccessTests;

public abstract class DataAccessFixtureBase<TDataAccessInterface>
    where TDataAccessInterface : class
{
    protected ITestDataAccess TestDataAccess { get; private set; } = null!;
    protected TDataAccessInterface BusDataAccess { get; private set; } = null!;
    protected ITestConfig TestConfig { get; private set; } = null!;
    protected IServiceScope Scope { get; set; } = null!;

    public virtual void Initialize()
    {
        Scope = TestServices.ServiceProvider.CreateScope();
        TestConfig = Scope.ServiceProvider.GetRequiredService<ITestConfig>();
        BusDataAccess = Scope.ServiceProvider.GetRequiredService<TDataAccessInterface>();
        TestDataAccess = Scope.ServiceProvider.GetRequiredService<ITestDataAccess>();
        TestDataAccess.Initialize();
        TestDataAccess.CleanEverything();
    }

    public virtual void Cleanup()
    {
        TestDataAccess.CloseConnections();
        Scope.Dispose();
    }

    protected static void Repeat(Action action, int count)
    {
        for (var i = 0; i < count; i++)
        {
            action();
        }
    }

    protected static async Task Repeat(Func<Task> action, int count)
    {
        for (var i = 0; i < count; i++)
        {
            await action();
        }
    }
}