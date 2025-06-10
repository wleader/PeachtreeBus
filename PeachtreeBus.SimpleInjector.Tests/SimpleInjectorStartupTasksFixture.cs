using PeachtreeBus.Tasks;
using SimpleInjector;
using System.Reflection;

namespace PeachtreeBus.SimpleInjector.Tests;

[TestClass]
public class SimpleInjectorStartupTasksFixture : SimpleInjectorExtensionFixtureBase
{
    public class FakeRunStartup : IRunStartupTasks
    {
        public int RunCount { get; private set; }
        public void RunStartupTasks() => RunCount++;
    }

    private void CustomizeContainer(Container container)
    {
        container.Register(typeof(IRunStartupTasks), typeof(FakeRunStartup), Lifestyle.Singleton);
    }

    private void Then_RunCountIs(int expected)
    {
        var actual = _container.GetInstance<IRunStartupTasks>();
        Assert.AreEqual(expected, ((FakeRunStartup)actual).RunCount);
    }

    [TestMethod]
    public void Given_NoStartupTasks_When_Run_Then_Runs()
    {
        var config = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample",
            Schema = new("PeachTreeBus"),
        };
        When_RunPeachtreeBus(config, [], CustomizeContainer);
        Then_RunCountIs(1);
    }


    [TestMethod]
    public void Given_StartupTasks_When_Run_Then_Runs()
    {
        var config = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample",
            Schema = new("PeachTreeBus"),
        };
        When_RunPeachtreeBus(config, [Assembly.GetExecutingAssembly()], CustomizeContainer);
        Then_RunCountIs(1);
    }
}
