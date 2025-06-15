using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Subscriptions;
using System;
using System.Collections.Generic;

namespace PeachtreeBus.Abstractions.Tests;

[TestClass]
public class SubscriptionConfigurationFixture : BaseConfigurationFixture<SubscriptionConfiguration>
{
    protected override SubscriptionConfiguration CreateConfiguration(bool useDefaults) =>
        new()
        {
            SubscriberId = AbstractionsTestData.DefaultSubscriberId,
            Topics = [],
            UseDefaultFailedHandler = useDefaults,
            UseDefaultRetryStrategy = useDefaults,
        };

    [TestMethod]
    public void Then_TopicsIsInit()
    {
        List<Topic> expected = [];
        CollectionAssert.AreEqual(expected, _config.Topics);
    }

    [TestMethod]
    public void Then_SubscriberIdIsInit()
    {
        Assert.AreEqual(AbstractionsTestData.DefaultSubscriberId,
            _config.SubscriberId);
    }

    [TestMethod]
    public void Then_LifespanIsReadWrite()
    {
        var timespan = TimeSpan.FromSeconds(12);
        _config.Lifespan = timespan;
        Assert.AreEqual(timespan, _config.Lifespan);
    }
}
