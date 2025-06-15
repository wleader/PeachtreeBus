using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Errors;

namespace PeachtreeBus.Abstractions.Tests.Errors;

[TestClass]
public class FailureCountFixture
{
    [TestMethod]
    public void Given_Uninitialzed_Then_ValueThrows()
    {
        FailureCount count = default;
        Assert.ThrowsException<FailureCountException>(
            () => _ = count.Value);
    }

    [TestMethod]
    public void Given_Uninitialized_Then_ToStringThrows()
    {
        FailureCount count = default;
        Assert.ThrowsException<FailureCountException>(
            () => _ = count.ToString());
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(10)]
    [DataRow(int.MaxValue)]
    public void Given_Valid_When_ThrowIfInvalid_Returns(int value)
    {
        Assert.AreEqual(value, new FailureCount(value).Value);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(-2)]
    [DataRow(-10)]
    [DataRow(int.MinValue)]
    public void Given_Invalid_When_ThrowIfInvalid_Throws(int value)
    {
        Assert.ThrowsException<FailureCountException>(
            () => _ = new FailureCount(value));
    }

    [TestMethod]
    public void Given_Byte_When_Assing_Then_ImplicitConverion()
    {
        byte value = 5;
        FailureCount count = value;
        Assert.AreEqual(5, count.Value);
    }

    [TestMethod]
    public void Given_FailureCount_When_AssignToInt_Then_ImplicitConversion()
    {
        var count = new FailureCount(5);
        int value = count;
        Assert.AreEqual(5, value);
    }
}

[TestClass]
public class FailureCountExceptionFixture
{
    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(10)]
    [DataRow(int.MaxValue)]
    public void Given_Valid_When_ThrowIfInvalid_Returns(int value)
    {
        Assert.AreEqual(value, FailureCountException.ThrowIfInvalid(value));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(-2)]
    [DataRow(-10)]
    [DataRow(int.MinValue)]
    public void Given_Invalid_When_ThrowIfInvalid_Throws(int value)
    {
        Assert.ThrowsException<FailureCountException>(
            () => FailureCountException.ThrowIfInvalid(value));
    }
}