using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Core.Tests.Subscriptions;

[TestClass]
public class ProcessSubscribedEstimatorFixture : ProcessEstimatorFixtureBase<ProcessSubscribedEstimator>
{
    [TestInitialize]
    public void Intialize()
    {
        _dataAccess.Reset();
        _busConfiguration.Reset();
        _estimator = new(_dataAccess.Object, _busConfiguration.Object);
    }

    protected override void Given_Configuration() => _busConfiguration.Given_SubscriptionConfiguration();
    protected override void Given_NoConfiguration() => _busConfiguration.Given_NoSubscriptionConfiguration();
    protected override void Given_DataAccessResult(long value) =>
        _dataAccess.Setup(x => x.EstimateSubscribedPending(It.IsAny<SubscriberId>()))
            .ReturnsAsync(value);
    protected override void Given_DataAccessThrows<T>(T exception) =>
        _dataAccess.Setup(x => x.EstimateSubscribedPending(It.IsAny<SubscriberId>())).ThrowsAsync(exception);
    protected override void VerifyDataAccessArguments() =>
        _dataAccess.Verify(d => d.EstimateSubscribedPending(
            _busConfiguration.Object.SubscriptionConfiguration!.SubscriberId),
            Times.Once);
}
