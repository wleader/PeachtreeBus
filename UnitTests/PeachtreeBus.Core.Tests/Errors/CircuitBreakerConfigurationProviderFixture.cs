using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Errors;

[TestClass]
public class CircuitBreakerConfigurationProviderFixture
{
    private CircuitBreakerConfigurationProvider _provider = default!;
    private Mock<IBusConfiguration> _busConfig = new();

    [TestInitialize]
    public void Initialize()
    {
        _busConfig.Reset();

        _busConfig.SetupGet(x => x.ConnectionString)
            .Returns("CONNECTIONSTRING");


        _provider = new(
            _busConfig.Object);
    }

    [TestMethod]
    public void Given_BusDataKey_When_Get_Then_BusDataBreakerConfig()
    {
        var key = new BreakerKey(BreakerType.DatabaseConnection, "CONNECTIONSTRING");
        var actual = _provider.Get(key);
        Assert.IsNotNull(actual);
        Assert.AreEqual("Bus Database Connection", actual.FriendlyName);
        Assert.AreEqual(TimeSpan.FromSeconds(5), actual.ArmedDelay);
        Assert.AreEqual(TimeSpan.FromSeconds(30), actual.FaultedDelay);
        Assert.AreEqual(TimeSpan.FromSeconds(30), actual.TimeToFaulted);
    }

    [TestMethod]
    public void Given_OtherKey_When_Get_Then_DefaultBreakerConfiguration()
    {
        var key = new BreakerKey(BreakerType.DatabaseConnection, "OtherString");
        var actual = _provider.Get(key);
        Assert.IsNotNull(actual);
        Assert.AreEqual("Default Breaker", actual.FriendlyName);
        Assert.AreEqual(TimeSpan.FromSeconds(1), actual.ArmedDelay);
        Assert.AreEqual(TimeSpan.FromSeconds(10), actual.FaultedDelay);
        Assert.AreEqual(TimeSpan.FromSeconds(30), actual.TimeToFaulted);
    }
}
