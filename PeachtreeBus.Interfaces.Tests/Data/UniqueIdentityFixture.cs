using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using System;
using System.Text.Json;

namespace PeachtreeBus.Interfaces.Tests.Data;

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
}
