using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Model;
using System;

namespace PeachtreeBus.Tests.Model;

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
