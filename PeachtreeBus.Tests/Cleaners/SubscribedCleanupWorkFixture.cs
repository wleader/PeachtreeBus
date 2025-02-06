using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Cleaners;
using PeachtreeBus.Interfaces;

namespace PeachtreeBus.Tests.Cleaners;

[TestClass]
public class SubscribedCleanupWorkFixture
{
    [TestMethod]
    public void When_New_Then_Created()
    {
        var config = new Mock<IBusConfiguration>();
        var clock = new Mock<ISystemClock>();
        var cleaner = new Mock<ISubscribedCleaner>();
        var work = new SubscribedCleanupWork(config.Object, clock.Object, cleaner.Object);
        Assert.IsNotNull(work);
    }
}
