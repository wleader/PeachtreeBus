using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using RetryResult = PeachtreeBus.Errors.RetryResult;

namespace PeachtreeBus.Abstractions.Tests.Errors;

[TestClass]
public class RetryResultFixture
{
    [TestMethod]
    public void When_New_Then_PropertiesAreRead()
    {
        var delay = TimeSpan.FromSeconds(13);
        var result = new RetryResult(true, delay);
        Assert.IsTrue(result.ShouldRetry);
        Assert.AreEqual(delay, result.Delay);
    }

    [TestMethod]
    public void Given_DelayIsNegative_When_New_Then_Throws()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => new RetryResult(true, new TimeSpan(-1)));
    }
}
