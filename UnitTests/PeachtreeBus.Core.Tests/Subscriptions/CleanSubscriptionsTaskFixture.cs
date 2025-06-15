using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Subscriptions;

[TestClass]
public class CleanSubscriptionsTaskFixture
{
    private CleanSubscriptionsTask _task = default!;
    private readonly Mock<IBusConfiguration> _busConfiguration = new();
    private readonly Mock<IBusDataAccess> _dataAccess = new();

    [TestInitialize]
    public void Initialize()
    {
        _busConfiguration.Reset();
        _dataAccess.Reset();

        _busConfiguration.Given_SubscriptionConfiguration();
        _dataAccess.DisallowTransactions();
        _dataAccess.Setup(d => d.ExpireSubscriptions(It.IsAny<int>()))
            .ReturnsAsync(100);

        _task = new(
            _busConfiguration.Object,
            _dataAccess.Object);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _dataAccess.Verify();
    }

    [TestMethod]
    public async Task Given_Configuration_When_DoWork_Then_ExpireSubscriptions()
    {
        var maxRows = _busConfiguration.Object.SubscriptionConfiguration!.CleanMaxRows;
        await _task.RunOne();
        _dataAccess.Verify(d => d.ExpireSubscriptions(maxRows), Times.Once);
    }

    [TestMethod]
    public async Task Given_NoConfiguration_When_DoWork_Then_NoWorkDone()
    {
        _busConfiguration.Given_NoSubscriptionConfiguration();
        Assert.IsFalse(await _task.RunOne());
        _dataAccess.Verify(d => d.ExpireSubscriptions(It.IsAny<int>()), Times.Never);
    }

    [TestMethod]
    public async Task Given_ExpireResults_When_DoWorkRepeatedly_Then_Results()
    {
        Queue<long> expireResult = new([100, 1, 0, 0]);
        _dataAccess.Setup(d => d.ExpireSubscriptions(It.IsAny<int>()))
            .ReturnsAsync(expireResult.Dequeue);

        Assert.IsTrue(await _task.RunOne());
        Assert.IsTrue(await _task.RunOne());
        Assert.IsFalse(await _task.RunOne());
    }
}
