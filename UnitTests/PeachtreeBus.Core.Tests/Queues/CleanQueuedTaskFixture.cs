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
    [DataRow(0, false)]
    [DataRow(1, true)]
    [DataRow(100, true)]
    [DataRow(1000, true)]
    public async Task Given_Configuration_And_RowsCleaned_When_Run_Then_Result(int rowsCleaned, bool expectedResult)
    {
        _busConfiguration.Given_QueueConfiguration();
        var c = _busConfiguration.Object.QueueConfiguration!;

        _dataAccess.Setup(d => d.CleanQueueCompleted(
            c.QueueName, It.IsAny<UtcDateTime>(), It.IsAny<int>()))
            .ReturnsAsync(() => rowsCleaned);
        Assert.AreEqual(expectedResult, await _task.RunOne());
        var expectedOlderThan = _clock.UtcNow.Subtract(c!.CleanCompleteAge);
        var expectedMaxRows = c.CleanMaxRows;
        _dataAccess.Verify(d => d.CleanQueueCompleted(c.QueueName, expectedOlderThan, expectedMaxRows), Times.Once);
    }
}
