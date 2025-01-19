using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using System;

namespace PeachtreeBus.Tests.Data;

[TestClass]
public class UtcDateTimeFixture
{
    [TestMethod]
    public void Given_UnspecifiedDateTime_When_Constructor_Then_Throws()
    {
        Assert.ThrowsException<ArgumentException>(() =>
        {
            _ = new UtcDateTime(DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified));
        });
    }
}
