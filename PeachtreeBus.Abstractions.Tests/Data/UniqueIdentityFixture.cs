using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using System;
using System.Text.Json;

namespace PeachtreeBus.Absractions.Tests.Data;

[TestClass]
public class UniqueIdentityFixture
{
    [TestMethod]
    public void Given_GuidEmpty_When_New_Then_Throws()
    {
        Assert.ThrowsException<UniqueIdentityException>(() =>
            _ = new UniqueIdentity(Guid.Empty));
    }

    [TestMethod]
    public void Given_Guid_When_New_Then_Value()
    {
        const string expectedStr = "5b792887-458d-4965-96d0-b824aef8bfa3";
        var expected = Guid.Parse(expectedStr);
        var actual = new UniqueIdentity(expected);
        Assert.AreEqual(expected, actual.Value);
        Assert.AreEqual(expectedStr, actual.ToString());
    }

    [TestMethod]
    [DataRow("5b792887-458d-4965-96d0-b824aef8bfa3", "\"5b792887-458d-4965-96d0-b824aef8bfa3\"")]
    public void Given_Value_When_RoundTripJson_Then_Value(string guid, string expectedJson)
    {
        UniqueIdentity expected = new(Guid.Parse(guid));
        var serialized = JsonSerializer.Serialize(expected);
        var actual = JsonSerializer.Deserialize<UniqueIdentity>(serialized);
        Assert.AreEqual(expectedJson, serialized);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void Then_EmptyIsGuidEmpty()
    {
        Assert.AreEqual(Guid.Empty, UniqueIdentity.Empty.Value);
    }

    [TestMethod]
    public void When_New_Then_Result()
    {
        Assert.AreNotEqual(Guid.Empty, UniqueIdentity.New().Value);
    }

    [TestMethod]
    public void Given_Invalid_When_RequireValid_Then_Throws()
    {
        Assert.ThrowsException<UniqueIdentityException>(
            () => UniqueIdentity.Empty.RequireValid());
    }

    [TestMethod]
    public void GivenValid_When_RequireValid_Then_Returns()
    {
        var expected = UniqueIdentity.New();
        var actual = expected.RequireValid();
        Assert.AreEqual(expected, actual);
    }
}
