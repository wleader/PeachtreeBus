using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Queues;


[TestClass]
public class CleanQueuedCompletedTaskFixture
{
    private CleanQueuedCompletedTask _task = default!;
    private readonly Mock<IBusConfiguration> _busConfiguration = new();
    private readonly Mock<IBusDataAccess> _dataAccess = new();
    private readonly FakeClock _clock = new();

    [TestInitialize]
    public void Initialize()
    {
        _busConfiguration.Reset();
        _dataAccess.Reset();
        _clock.Reset();

        _task = new(
            _busConfiguration.Object,
            _dataAccess.Object,
            _clock);
    }

    [TestMethod]
    public async Task Given_NoConfiguration_When_Run_Then_False()
    {
        _busConfiguration.Given_NoQueueConfiguration();
        Assert.IsFalse(await _task.RunOne());
    }

    [TestMethod]
    public async Task Given_CleanCompletedFalse_When_Run_Then_ReturnsFalse_and_DataAccessNotInvoked()
    {
        var queueConfig = _busConfiguration.Given_QueueConfiguration();
        queueConfig.CleanCompleted = false;

        Assert.IsFalse(await _task.RunOne());
        _dataAccess.Verify(d => d.CleanQueueCompleted(
            It.IsAny<QueueName>(),
            It.IsAny<UtcDateTime>(),
            It.IsAny<int>()), Times.Never);
        _dataAccess.VerifyNoOtherCalls();
    }

    [TestMethod]
    [DataRow(0, false)]
    [DataRow(1, true)]
    [DataRow(100, true)]
    [DataRow(1000, true)]
    public async Task Given_Configuration_And_RowsCleaned_When_Run_Then_Result(int rowsCleaned, bool expectedResult)
    {
        var c = _busConfiguration.Given_QueueConfiguration();
        // check that the ages are different to make sure the older than is calculated from the correct configuration.
        Assert.AreNotEqual(c.CleanCompleteAge, c.CleanFailedAge);

        _dataAccess.Setup(d => d.CleanQueueCompleted(
            c.QueueName, It.IsAny<UtcDateTime>(), It.IsAny<int>()))
            .ReturnsAsync(() => rowsCleaned);
        Assert.AreEqual(expectedResult, await _task.RunOne());
        var expectedOlderThan = _clock.UtcNow.Subtract(c!.CleanCompleteAge);
        var expectedMaxRows = c.CleanMaxRows;
        _dataAccess.Verify(d => d.CleanQueueCompleted(c.QueueName, expectedOlderThan, expectedMaxRows), Times.Once);
    }
}
