using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Queues;

namespace PeachtreeBus.Core.Tests.Queues;

[TestClass]
public class ProcessQueuedEstimatorFixture : ProcessEstimatorFixtureBase<ProcessQueuedEstimator>
{
    [TestInitialize]
    public void Intialize()
    {
        _dataAccess.Reset();
        _busConfiguration.Reset();
        _estimator = new(_dataAccess.Object, _busConfiguration.Object);
    }

    protected override void Given_Configuration() => _busConfiguration.Given_QueueConfiguration();
    protected override void Given_NoConfiguration() => _busConfiguration.Given_NoQueueConfiguration();

    protected override void Given_DataAccessResult(long value) =>
        _dataAccess.Setup(x => x.EstimateQueuePending(It.IsAny<QueueName>()))
            .ReturnsAsync(value);

    protected override void Given_DataAccessThrows<T>(T exception) =>
        _dataAccess.Setup(x => x.EstimateQueuePending(It.IsAny<QueueName>())).ThrowsAsync(exception);

    protected override void VerifyDataAccessArguments() =>
        _dataAccess.Verify(d => d.EstimateQueuePending(_busConfiguration.Object.QueueConfiguration!.QueueName), Times.Once);


}
