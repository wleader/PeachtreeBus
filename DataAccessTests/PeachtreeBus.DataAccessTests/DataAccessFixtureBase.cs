using System;
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
    protected IServiceScope Scope { get; private set; } = null!;

    public virtual async Task Initialize()
    {
        Scope = TestServices.ServiceProvider.CreateScope();
        TestConfig = Scope.ServiceProvider.GetRequiredService<ITestConfig>();
        BusDataAccess = Scope.ServiceProvider.GetRequiredService<TDataAccessInterface>();
        TestDataAccess = Scope.ServiceProvider.GetRequiredService<ITestDataAccess>();
        await TestDataAccess.Initialize();
        await TestDataAccess.CleanEverything();
    }

    public virtual async Task Cleanup()
    {
        await TestDataAccess.CloseConnections();
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