using SimpleInjector;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.SimpleInjector.Tests;

[TestClass]
public class SimpleInjectorScopeFactory_GetImplementations_Fixture
    : SimpleInjectorScopeFactory_FixtureBase
{
    [ExcludeFromCodeCoverage(Justification = "Non Shipping Test Class")]
    public class StartupTask1 : IRunOnStartup
    {
        public Task Run() => Task.CompletedTask;
    }

    [ExcludeFromCodeCoverage(Justification = "Non Shipping Test Class")]
    public class StartupTask2 : IRunOnStartup
    {
        public Task Run() => Task.CompletedTask;
    }

    [TestMethod]
    public void Given_MulitpleRegistrations_When_GetImplementations_Then_Result()
    {
        _container.Register(typeof(StartupTask1), typeof(StartupTask1), Lifestyle.Scoped);
        _container.Register(typeof(StartupTask2), typeof(StartupTask2), Lifestyle.Scoped);
        _container.Verify();
        var actual = _factory.GetImplementations<IRunOnStartup>().ToList();
        Assert.AreEqual(2, actual.Count);
        CollectionAssert.Contains(actual, typeof(StartupTask1));
        CollectionAssert.Contains(actual, typeof(StartupTask2));
    }
}
