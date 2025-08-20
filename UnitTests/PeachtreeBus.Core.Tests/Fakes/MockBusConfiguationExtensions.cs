using Moq;

namespace PeachtreeBus.Core.Tests.Fakes;

public static class MockBusConfiguationExtensions
{
    public static void Given_NoQueueConfiguration(this Mock<IBusConfiguration> config)
    {
        config.SetupGet(c => c.QueueConfiguration).Returns((QueueConfiguration)null!);
    }

    public static QueueConfiguration Given_QueueConfiguration(this Mock<IBusConfiguration> config)
    {
        var result = TestData.CreateQueueConfiguration();
        config.SetupGet(c => c.QueueConfiguration).Returns(() => result);
        return result;
    }

    public static void Given_NoSubscriptionConfiguration(this Mock<IBusConfiguration> config)
    {
        config.SetupGet(c => c.SubscriptionConfiguration).Returns((SubscriptionConfiguration)null!);
    }

    public static SubscriptionConfiguration Given_SubscriptionConfiguration(this Mock<IBusConfiguration> config)
    {
        var result = TestData.CreateSubscriptionConfiguration();
        config.SetupGet(c => c.SubscriptionConfiguration).Returns(() => result);
        return result;
    }

}
