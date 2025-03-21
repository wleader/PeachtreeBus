using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace PeachtreeBus.Abstractions.Tests;

[TestClass]
public class PublishConfigurationFixture
{
    [TestMethod]
    public void Then_LifespanIsReadWrite()
    {
        var config = new PublishConfiguration();
        var lifespan = TimeSpan.FromMinutes(60);
        config.Lifespan = lifespan;
        Assert.AreEqual(lifespan, config.Lifespan);
    }

    [TestMethod]
    public void Then_LifespanDefaults()
    {
        var config = new PublishConfiguration();
        Assert.AreEqual(TimeSpan.FromDays(1), config.Lifespan);
    }
}
