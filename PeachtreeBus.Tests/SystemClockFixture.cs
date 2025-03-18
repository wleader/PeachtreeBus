using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace PeachtreeBus.Tests;

[TestClass]
public class SystemClockFixture
{
    [TestMethod]
    public void When_UtcNow_Then_ResultKindIsUtc()
    {
        var clock = new SystemClock();
        Assert.AreEqual(DateTimeKind.Utc, clock.UtcNow.Kind);
    }
}
