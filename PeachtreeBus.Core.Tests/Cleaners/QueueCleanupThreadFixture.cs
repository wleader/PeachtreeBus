using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Cleaners;
using PeachtreeBus.Data;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Cleaners;

[TestClass]
public class QueueCleanupThreadFixture : ThreadFixtureBase<QueueCleanupThread>
{
    private readonly Mock<ILogger<QueueCleanupThread>> log = new();
    private readonly Mock<IBusDataAccess> dataAccess = new();
    private readonly Mock<IQueueCleanupWork> cleaner = new();

    [TestInitialize]
    public void TestInitialize()
    {
        log.Reset();
        dataAccess.Reset();
        cleaner.Reset();

        cleaner.Setup(c => c.DoWork())
            .Callback(CancelToken)
            .ReturnsAsync(true);

        _testSubject = new QueueCleanupThread(
            log.Object,
            dataAccess.Object,
            cleaner.Object);
    }

    /// <summary>
    /// Prove the cleaner is invoked.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task When_Run_Then_WorkRuns()
    {
        await When_Run();
        cleaner.Verify(c => c.DoWork(), Times.Once);
    }
}
