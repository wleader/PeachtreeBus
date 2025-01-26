using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;

namespace PeachtreeBus.Tests.Data;

[TestClass]
public class IdentityFixture
{
    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(long.MinValue)]
    public void Given_Long_When_New_Then_Throws(long value)
    {
        Assert.ThrowsException<IdentityException>(() => _ = new Identity(value));
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(123)]
    [DataRow(long.MaxValue)]
    public void Given_Value_When_New_Then_Value(long value)
    {
        var actual = new Identity(value);
        Assert.AreEqual(value, actual.Value);
        Assert.AreEqual(value.ToString(), actual.ToString());
    }
}
