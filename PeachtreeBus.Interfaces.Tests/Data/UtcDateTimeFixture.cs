using PeachtreeBus.Data;
using System;
using System.Text.Json;

namespace PeachtreeBus.Interfaces.Tests.Data;

[TestClass]
public class UtcDateTimeFixture
{
    [TestMethod]
    public void Given_UnspecifiedDateTime_When_Constructor_Then_Throws()
    {
        Func<UtcDateTime> func = new(() => new UtcDateTime(DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)));
        Assert.ThrowsException<ArgumentException>(() => func());
    }

    [TestMethod]
    [DataRow("2009-05-01T14:57:32.8375298-04:00", "\"2009-05-01T18:57:32.8375298Z\"")]
    public void Given_Value_When_RoundTripJson_Then_Value(string strValue, string expectedJson)
    {
        // prove that the custom JsonConverter works as intended.
        Assert.IsTrue(DateTime.TryParse(strValue, out var value));
        var expected = new UtcDateTime(value);
        var serialized = JsonSerializer.Serialize(expected);
        var actual = JsonSerializer.Deserialize<UtcDateTime>(serialized);
        Assert.AreEqual(expectedJson, serialized);
        Assert.AreEqual(expected, actual);
    }
}
