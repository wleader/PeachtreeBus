using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace PeachtreeBus.Abstractions.Tests;

public abstract class BaseConfigurationFixture<TConfiguration>
    where TConfiguration : BaseConfiguration
{
    protected TConfiguration _config = default!;

    [TestInitialize]
    public void Intialize()
    {
        _config = CreateConfiguration();
    }

    protected abstract TConfiguration CreateConfiguration(bool useDefaults = true);

    [TestMethod]
    public void Then_PropertiesAreReadWrite()
    {
        _config.CleanCompleted = false;
        Assert.IsFalse(_config.CleanCompleted);

        _config.CleanFailed = false;
        Assert.IsFalse(_config.CleanFailed);

        _config.CleanMaxRows = 200;
        Assert.AreEqual(200, _config.CleanMaxRows);

        var timespan = TimeSpan.FromSeconds(10);

        _config.CleanFailedAge = timespan;
        Assert.AreEqual(timespan, _config.CleanFailedAge);

        _config.CleanInterval = timespan;
        Assert.AreEqual(timespan, _config.CleanInterval);

        _config.CleanCompleteAge = timespan;
        Assert.AreEqual(timespan, _config.CleanCompleteAge);
    }

    [TestMethod]
    [DataRow(true, DisplayName = "UseDefaults")]
    [DataRow(false, DisplayName = "UseCustom")]
    public void Then_UseDefaultsAreInit(bool useDefaults)
    {
        var c = CreateConfiguration(useDefaults);
        Assert.AreEqual(useDefaults, c.UseDefaultFailedHandler);
        Assert.AreEqual(useDefaults, c.UseDefaultRetryStrategy);
    }
}
