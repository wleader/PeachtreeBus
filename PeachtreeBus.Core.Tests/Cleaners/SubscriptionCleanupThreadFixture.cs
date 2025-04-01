using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Cleaners;
using PeachtreeBus.Data;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Cleaners;

[TestClass]
public class SubscriptionCleanupThreadFixture : ThreadFixtureBase<SubscriptionCleanupThread>
{
    private Mock<ISystemClock> clock = default!;
    private Mock<ISubscriptionCleanupWork> cleaner = default!;
    private Mock<ILogger<SubscriptionCleanupThread>> log = default!;
    private Mock<IBusDataAccess> dataAccess = default!;

    [TestInitialize]
    public void TestInitialize()
    {
        clock = new Mock<ISystemClock>();
        clock.SetupGet(c => c.UtcNow)
            .Returns(new DateTime(2022, 3, 4, 10, 49, 32, 33, DateTimeKind.Utc));

        cleaner = new Mock<ISubscriptionCleanupWork>();
        log = new Mock<ILogger<SubscriptionCleanupThread>>();
        dataAccess = new Mock<IBusDataAccess>();

        _testSubject = new SubscriptionCleanupThread(
            dataAccess.Object,
            log.Object,
            cleaner.Object,
            clock.Object);
    }

    /// <summary>
    /// Proves the cleanup thread doesn't clean too often
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task DoUnitOfWork_DoesntRunTooOften()
    {
        _testSubject.LastCleaned = clock.Object.UtcNow;
        Assert.IsFalse(await _testSubject.DoUnitOfWork());
        cleaner.Verify(c => c.DoWork(), Times.Never);

        clock.SetupGet(c => c.UtcNow)
            .Returns(_testSubject.LastCleaned.AddSeconds(16));

        Assert.IsTrue(await _testSubject.DoUnitOfWork());
        cleaner.Verify(c => c.DoWork(), Times.Once);

        Assert.AreEqual(clock.Object.UtcNow, _testSubject.LastCleaned);
    }
}
