using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using System;
using System.Text.Json;

namespace PeachtreeBus.Abstractions.Tests.Data;

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
    public void Given_Uninitialized_When_Value_Then_Utc()
    {
        UtcDateTime uninitialized = default;
        Assert.AreEqual(DateTimeKind.Utc, uninitialized.Value.Kind);
        Assert.AreEqual(0, uninitialized.Value.Ticks);
    }

    [TestMethod]
    [DataRow("\"2009-05-01T14:57:32.8375298-04:00\"", "\"2009-05-01T18:57:32.8375298Z\"")]
    [DataRow("\"0001-01-01T00:00:00.0000000Z\"", "\"0001-01-01T00:00:00Z\"")]
    [DataRow("\"0001-01-01T00:00:00Z\"", "\"0001-01-01T00:00:00Z\"")]
    public void Given_Value_When_RoundTripJson_Then_Value(string strValue, string expectedJson)
    {
        // prove that the custom JsonConverter works as intended.
        var value = JsonSerializer.Deserialize<DateTime>(strValue);
        var expected = new UtcDateTime(value);
        var serialized = JsonSerializer.Serialize(expected);
        var actual = JsonSerializer.Deserialize<UtcDateTime>(serialized);
        Assert.AreEqual(expectedJson, serialized);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void When_ImplicitCast_Then_Result()
    {
        var expected = DateTime.UtcNow;
        UtcDateTime actual = expected;
        DateTime expected2 = actual;

        Assert.AreEqual(expected, actual.Value);
        Assert.AreEqual(expected2, actual.Value);
    }

    [TestMethod]
    public void Given_Value_When_Add_Then_Result()
    {
        var original = DateTime.UtcNow;
        UtcDateTime utcOriginal = original;
        Assert.AreEqual(original.AddMinutes(1), utcOriginal.AddMinutes(1).Value);
        Assert.AreEqual(original.AddHours(1), utcOriginal.AddHours(1).Value);
        Assert.AreEqual(original.AddMilliseconds(1), utcOriginal.AddMilliseconds(1).Value);
        Assert.AreEqual(original.AddDays(1), utcOriginal.AddDays(1).Value);
    }

    [TestMethod]
    public void Given_Value_When_ToString_Then_Result()
    {
        var original = DateTime.UtcNow;
        var expected = original.ToString();
        UtcDateTime utcOriginal = original;
        Assert.AreEqual(expected, utcOriginal.ToString());
    }
}
