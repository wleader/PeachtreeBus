using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Cleaners;
using System;

namespace PeachtreeBus.Tests.Cleaners;

[TestClass]
public class SubscribedCleanupConfigurationFixture
{
    [TestMethod]
    public void When_New_Then_PropertiesAreSet()
    {
        var ageLimit = TimeSpan.FromMinutes(5);
        var interval = TimeSpan.FromMinutes(1);
        var config = new SubscribedCleanupConfiguration(1, true, false, ageLimit, interval);
        Assert.AreEqual(1, config.MaxDeleteCount);
        Assert.IsTrue(config.CleanCompleted);
        Assert.IsFalse(config.CleanFailed);
        Assert.AreEqual(ageLimit, config.AgeLimit);
        Assert.AreEqual(interval, config.Interval);
    }
}
