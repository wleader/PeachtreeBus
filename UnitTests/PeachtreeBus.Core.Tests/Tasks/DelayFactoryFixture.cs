using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Tasks;

[TestClass]
public class DelayFactoryFixture
{
    private readonly DelayFactory _factory = new();

    [TestMethod]
    public async Task Given_TimespanZero_When_Delay_Then_TaskIsCompleted()
    {
        var cts = new CancellationTokenSource();
        var actual = _factory.Delay(TimeSpan.Zero, cts.Token);
        Assert.IsTrue(actual.IsCompleted);
        await actual;
    }

    [TestMethod]
    public async Task Given_Timespan_When_Delay_Then_TaskIsCancellable()
    {
        var cts = new CancellationTokenSource();
        var actual = _factory.Delay(TimeSpan.FromMinutes(1), cts.Token);
        Assert.IsFalse(actual.IsCompleted);
        Assert.IsFalse(actual.IsCanceled);
        cts.Cancel();
        Assert.IsTrue(actual.IsCanceled);
        await Assert.ThrowsAsync<TaskCanceledException>(() => actual);
    }

    [TestMethod]
    public async Task Given_Timespan_When_Delay_Then_TaskIsADelayPromise()
    {
        var cts = new CancellationTokenSource();
        var actual = _factory.Delay(TimeSpan.FromMilliseconds(20), cts.Token);
        Assert.AreEqual("DelayPromiseWithCancellation", actual.GetType().Name);
        await actual;
    }
}
