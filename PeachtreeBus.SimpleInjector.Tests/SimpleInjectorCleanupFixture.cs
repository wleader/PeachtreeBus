using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.SimpleInjector.Tests;

[TestClass]
public class SimpleInjectorCleanupFixture : SimpleInjectorExtensionFixtureBase
{
    [TestMethod]
    [DataRow(false, false)]
    [DataRow(false, true)]
    [DataRow(true, false)]
    [DataRow(true, true)]
    public void Given_QueueCleaning_When_Run_Then_Runs(bool cleanFailed, bool cleanComplete)
    {
        var config = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample",
            Schema = new("PeachTreeBus"),
            QueueConfiguration = new()
            {
                QueueName = new("QueueName"),
                CleanFailed = cleanFailed,
                CleanCompleted = cleanComplete
            },
        };

        _container.UsePeachtreeBus(config, _loggerFactory, _assemblies);
        _container.Verify();
        _container.RunPeachtreeBus();
    }

    [TestMethod]
    [DataRow(false, false)]
    [DataRow(false, true)]
    [DataRow(true, false)]
    [DataRow(true, true)]
    public void Given_SubscribedCleaning_When_Run_Then_Runs(bool cleanFailed, bool cleanComplete)
    {
        var config = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample",
            Schema = new("PeachTreeBus"),
            SubscriptionConfiguration = new()
            {
                SubscriberId = SubscriberId.New(),
                Topics = [],
                CleanFailed = cleanFailed,
                CleanCompleted = cleanComplete
            },
        };

        _container.UsePeachtreeBus(config, _loggerFactory, _assemblies);
        _container.Verify();
        _container.RunPeachtreeBus();
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void Given_SubscriptionCleaning_When_Run_Then_Runs(bool clean)
    {
        var config = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample;Integrated Security=True;Max Pool Size=500;TrustServerCertificate=True",
            Schema = new("PeachTreeBus"),
            SubscriptionConfiguration = new()
            {
                SubscriberId = SubscriberId.New(),
                Topics = [],
                //CleanSubscriptions = clean
            },
        };

        _container.UsePeachtreeBus(config, _loggerFactory, _assemblies);

        _container.Verify();
        _container.RunPeachtreeBus();

        Assert.Inconclusive($"clean={clean}: Subscription cleaning is not yet configurable.");
    }
}
