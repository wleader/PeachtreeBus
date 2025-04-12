using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Subscriptions;

[TestClass]
public class CleanSubscribedPendingTaskFixture
{
    private CleanSubscribedPendingTask _task = default!;
    private readonly Mock<IBusConfiguration> _busConfiguration = new();
    private readonly Mock<IBusDataAccess> _dataAccess = new();
    private const int ExpireResult = 100;

    [TestInitialize]
    public void Initialize()
    {
        _busConfiguration.Reset();
        _dataAccess.Reset();

        _busConfiguration.Given_SubscriptionConfiguration();
        _dataAccess.DisallowTransactions();
        _dataAccess.Setup(d => d.ExpireSubscriptionMessages(It.IsAny<int>()))
            .ReturnsAsync(() => ExpireResult);

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
        _dataAccess.Verify(d => d.ExpireSubscriptionMessages(maxRows), Times.Once);
    }

    [TestMethod]
    public async Task Given_NoConfiguration_When_DoWork_Then_NoWorkDone()
    {
        _busConfiguration.Given_NoSubscriptionConfiguration();
        Assert.IsFalse(await _task.RunOne());
        _dataAccess.Verify(d => d.ExpireSubscriptionMessages(It.IsAny<int>()), Times.Never);
    }

    [TestMethod]
    public async Task Given_ExpireResults_When_DoWorkRepeatedly_Then_Result()
    {
        Queue<long> results = new([100, 1, 0, 0]);
        _dataAccess.Setup(d => d.ExpireSubscriptionMessages(It.IsAny<int>()))
            .ReturnsAsync(results.Dequeue);
        Assert.IsTrue(await _task.RunOne());
        Assert.IsTrue(await _task.RunOne());
        Assert.IsFalse(await _task.RunOne());
    }
}
