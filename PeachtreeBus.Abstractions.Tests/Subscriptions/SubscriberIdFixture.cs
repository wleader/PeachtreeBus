using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Subscriptions;
using System;

namespace PeachtreeBus.Abstractions.Tests.Subscriptions;

[TestClass]
public class SubscriberIdFixture
{
    [TestMethod]
    public void Given_Guid_When_New_Then_Value()
    {
        var guid = Guid.NewGuid();
        var actual = new SubscriberId(guid);
        Assert.AreEqual(guid, actual.Value);
        Assert.AreEqual(guid.ToString(), actual.ToString());
    }

    [TestMethod]
    public void Given_GuidEmpty_When_New_Then_Throws()
    {
        Assert.ThrowsException<SubscriberIdException>(() =>
            new SubscriberId(Guid.Empty));
    }

    [TestMethod]
    public void Given_Invalid_When_RequireValid_Then_Throws()
    {
        Assert.ThrowsException<SubscriberIdException>(
            () => SubscriberId.Invalid.RequreValid());
    }

    [TestMethod]
    public void Given_Valid_When_RequireValid_Then_Returns()
    {
        var expected = SubscriberId.New();
        var actual = expected.RequreValid();
        Assert.AreEqual(expected, actual);
    }
}
