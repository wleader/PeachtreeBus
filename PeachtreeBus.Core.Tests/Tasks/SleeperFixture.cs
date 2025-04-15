using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Tasks;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Tasks;

[TestClass]
public class SleeperFixture
{
    private Sleeper _sleeper = default!;

    [TestInitialize]
    public void Intialize()
    {
        _sleeper = new();
    }

    private static async Task<TimeSpan> Measure(Task sleep)
    {
        var sw = new Stopwatch();
        sw.Start();
        await sleep;
        sw.Stop();
        return sw.Elapsed;
    }

    private static void AssertCloseEnough(TimeSpan expected, TimeSpan actual, TimeSpan? tolerance = null)
    {
        tolerance ??= TimeSpan.FromMilliseconds(20);
        Assert.AreEqual(expected.Ticks, actual.Ticks, tolerance.Value.Ticks);
    }

    [TestMethod]
    public async Task When_Sleep_Then_Sleeps()
    {
        var t = _sleeper.Sleep(500);
        var actual = await Measure(t);
        AssertCloseEnough(TimeSpan.FromMilliseconds(500), actual);
    }

    [TestMethod]
    public async Task Given_Sleeping_When_Wakeup_Then_WakesUp()
    {
        var t = _sleeper.Sleep(500);
        _sleeper.Wake();
        var actual = await Measure(t);
        Assert.IsTrue(10 > actual.TotalMilliseconds);
    }
}
