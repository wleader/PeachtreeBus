using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Cleaners;
using PeachtreeBus.Data;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Cleaners;

/// <summary>
/// Proves the behavior of SubscribedCleanupThread
/// </summary>
[TestClass]
public class SubscribedCleanupThreadFixture : ThreadFixtureBase<SubscribedCleanupThread>
{
    private readonly Mock<ILogger<SubscribedCleanupThread>> log = new();
    private readonly Mock<IBusDataAccess> dataAccess = new();
    private readonly Mock<ISubscribedCleanupWork> cleaner = new();

    [TestInitialize]
    public void TestInitialize()
    {
        log.Reset();
        dataAccess.Reset();
        cleaner.Reset();

        cleaner.Setup(x => x.DoWork())
            .Callback(CancelToken)
            .ReturnsAsync(true);

        _testSubject = new SubscribedCleanupThread(
            log.Object,
            dataAccess.Object,
            cleaner.Object);
    }

    /// <summary>
    /// Proves the cleaner is invoked.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task When_Run_Then_WorkRuns()
    {
        await When_Run();
        cleaner.Verify(c => c.DoWork(), Times.Once);
    }
}
