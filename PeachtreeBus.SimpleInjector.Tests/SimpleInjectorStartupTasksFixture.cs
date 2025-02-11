using System.Reflection;
using System.Threading.Tasks;

namespace PeachtreeBus.SimpleInjector.Tests;

[TestClass]
public class SimpleInjectorStartupTasksFixture : SimpleInjectorExtensionFixtureBase
{
    public class FindableStartupTask : IRunOnStartup
    {
        public static int RunCount { get; set; } = 0;
        public Task Run()
        {
            RunCount++;
            return Task.CompletedTask;
        }
    }

    [TestMethod]
    public void Given_NoStartupTasks_When_Run_Then_Runs()
    {
        var config = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample",
            Schema = new("PeachTreeBus"),
        };

        FindableStartupTask.RunCount = 0;
        _container.UsePeachtreeBus(config, _loggerFactory, []);
        _container.Verify();
        _container.RunPeachtreeBus();
        Assert.AreEqual(0, FindableStartupTask.RunCount);
    }

    [TestMethod]
    public void Given_StartupTasks_When_Run_Then_Runs()
    {
        var config = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample",
            Schema = new("PeachTreeBus"),
        };

        FindableStartupTask.RunCount = 0;
        _container.UsePeachtreeBus(config, _loggerFactory, [Assembly.GetExecutingAssembly()]);
        _container.Verify();
        _container.RunPeachtreeBus();
        Assert.AreEqual(1, FindableStartupTask.RunCount);
    }
}
