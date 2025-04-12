using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Subscriptions;

[TestClass]
public class UpdateSubscriptionsTaskFixture
{
    private UpdateSubscriptionsTask _task = default!;
    private readonly Mock<IBusDataAccess> _dataAccess = new();
    private readonly Mock<IBusConfiguration> _busConfiguration = new();
    private readonly FakeClock _clock = new();

    [TestInitialize]
    public void Initialize()
    {
        _dataAccess.Reset();
        _busConfiguration.Reset();
        _clock.Reset();

        _task = new(
            _dataAccess.Object,
            _busConfiguration.Object,
            _clock);
    }

    [TestMethod]
    public async Task Given_NoConfiguration_When_DoWork_Then_False()
    {
        _busConfiguration.Given_NoQueueConfiguration();
        Assert.IsFalse(await _task.RunOne());
    }

    [TestMethod]
    public async Task Given_Configuration_When_DoWork_Then_Subscribes_And_WorkDone()
    {
        var config = _busConfiguration.Given_SubscriptionConfiguration();

        var expectedUntil = _clock.UtcNow
            .Add(config.Lifespan);

        Assert.IsTrue(await _task.RunOne());
        Assert.IsTrue(config.Topics.Count > 0);
        foreach (var topic in config.Topics)
        {
            _dataAccess.Verify(d => d.Subscribe(config.SubscriberId, topic, expectedUntil), Times.Once);
        }
        Assert.AreEqual(_dataAccess.Invocations.Count, config.Topics.Count);

        // once it is complete, a second run returns false
        Assert.IsFalse(await _task.RunOne());
    }
}
