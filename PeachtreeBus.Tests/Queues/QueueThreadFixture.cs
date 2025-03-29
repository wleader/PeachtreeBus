using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Queues;

[TestClass]
public class QueueThreadFixture : ThreadFixtureBase<QueueThread>
{
    private readonly Mock<IBusDataAccess> dataAccess = new();
    private readonly Mock<ILogger<QueueThread>> log = new();
    private readonly Mock<IQueueWork> work = new();

    [TestInitialize]
    public void TestInitialize()
    {
        log.Reset();
        dataAccess.Reset();
        work.Reset();

        work.Setup(p => p.DoWork())
            .Callback(CancelToken)
            .ReturnsAsync(true);

        var config = TestData.CreateBusConfiguration();

        _testSubject = new QueueThread(
            dataAccess.Object,
            log.Object,
            work.Object,
            config);
    }

    /// <summary>
    /// Proves the unit of work is invoked
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task When_Run_Then_WorkRuns()
    {
        await When_Run();
        work.Verify(p => p.DoWork(), Times.Once);
    }
}
