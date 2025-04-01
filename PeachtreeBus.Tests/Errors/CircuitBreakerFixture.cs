using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Errors;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Errors;

[TestClass]
public class CircuitBreakerFixture
{
    private CircuitBreaker _breaker = default!;
    private Mock<ILogger<CircuitBreaker>> _log = default!;

    // if tests are failing intermittently, try increasing these values.
    private const double Timescale = 1.0;
    private static readonly TimeSpan Tolerance = TimeSpan.FromMilliseconds(2);

    private readonly CircuitBreakerConfiguraton _config =
        new("DefaultBreakerKey", "Default Breaker")
        {
            ArmedDelay = TimeSpan.FromMilliseconds(10 * Timescale),
            FaultedDelay = TimeSpan.FromMilliseconds(50 * Timescale),
            TimeToFaulted = TimeSpan.FromMilliseconds(50 * Timescale),
        };

    // Use an exception defined here, because then
    // the guard code must be catching System.Exception.
    private class TestException : Exception;


    [TestInitialize]
    public void Initialize()
    {
        _log = new();

        Assert.IsTrue(_config.FaultedDelay > _config.ArmedDelay, "Configuration Is Invalid.");

        _breaker = new CircuitBreaker(_log.Object, _config);
    }

    [TestCleanup]
    public void Clean()
    {
        _breaker.Dispose();
    }

    [TestMethod]
    public async Task Given_BreakerFaulted_When_Cancel_Then_DelayIsCancelled()
    {
        // the cancellation token should pass through to any delays.
        static Task Cancellable(CancellationToken token) { return Task.CompletedTask; }
        await FaultBreaker();
        var tokenSource = new CancellationTokenSource();
        var sw = new Stopwatch();
        sw.Start();
        var task = _breaker.Guard(Cancellable, tokenSource.Token);
        tokenSource.Cancel();
        await Assert.ThrowsExactlyAsync<TaskCanceledException>(() => task);
        sw.Stop();
        Assert.IsTrue(sw.Elapsed.Subtract(Tolerance) < _config.FaultedDelay);
    }

    [TestMethod]
    public async Task Given_BreakerCleared_When_Cancel_Then_BreakerIsCleared()
    {
        // a cancellation exception should not trigger a fault or arming.
        static Task Cancellable(CancellationToken token) { return Task.Delay(TimeSpan.FromMinutes(1), token); }
        var tokenSource = new CancellationTokenSource();
        var sw = new Stopwatch();
        sw.Start();
        var task = _breaker.Guard(Cancellable, tokenSource.Token);
        tokenSource.Cancel();
        await Assert.ThrowsExactlyAsync<TaskCanceledException>(() => task);
        sw.Stop();
        await Then_BreakerIsCleared();
    }

    [TestMethod]
    public async Task When_ReturnFailure_Then_Throws_And_BreakerIsArmed()
    {
        static Task<string> Fail() { throw new TestException(); }
        await Assert.ThrowsExactlyAsync<TestException>(
            () => _breaker.Guard(Fail));
        await Then_BreakerIsArmed();
    }


    [TestMethod]
    public async Task When_CancellableReturnFailure_Then_Throws_And_BreakerIsArmed()
    {
        static Task<int> Fail(CancellationToken token) { throw new TestException(); }
        await Assert.ThrowsExactlyAsync<TestException>(
            () => _breaker.Guard((t) => Fail(t), default));
        await Then_BreakerIsArmed();
    }

    [TestMethod]
    public async Task When_Failure_Then_Throws_And_BreakerIsArmed()
    {
        static Task Fail() { throw new TestException(); }
        await Assert.ThrowsExactlyAsync<TestException>(
            () => _breaker.Guard(Fail));
        await Then_BreakerIsArmed();
    }

    [TestMethod]
    public async Task When_CancellableFailure_Then_Throws_And_BreakerIsArmed()
    {
        static Task Fail(CancellationToken token) { throw new TestException(); }
        await Assert.ThrowsExactlyAsync<TestException>(
            () => _breaker.Guard(Fail, default));
        await Then_BreakerIsArmed();
    }

    [TestMethod]
    public async Task When_Succeed_Then_NoThrows_AndBreakerIsCleared()
    {
        static Task Succeed() { return Task.CompletedTask; }
        await _breaker.Guard(Succeed);
        await Then_BreakerIsCleared();
    }

    [TestMethod]
    public async Task When_CancellableSucceed_Then_NoThrows_AndBreakerIsCleared()
    {
        static Task Succeed(CancellationToken token) { return Task.CompletedTask; }
        await _breaker.Guard(Succeed, default);
        await Then_BreakerIsCleared();
    }

    [TestMethod]
    public async Task When_Succeed_Then_Result_AndBreakerIsCleared()
    {
        static Task<int> Succeed() { return Task.FromResult(42); }
        Assert.AreEqual(42, await _breaker.Guard(Succeed));
        await Then_BreakerIsCleared();
    }

    [TestMethod]
    public async Task When_CancellableSucceed_Then_Result_AndBreakerIsCleared()
    {
        static Task<int> Succeed(CancellationToken token) { return Task.FromResult(3); }
        Assert.AreEqual(3, await _breaker.Guard(Succeed, default));
        await Then_BreakerIsCleared();
    }

    [TestMethod]
    public async Task Given_ClearBreaker_Then_BreakerIsCleared()
    {
        await ClearBreaker();
        await Then_BreakerIsCleared();
    }

    [TestMethod]
    public async Task Given_ArmBreaker_Then_BreakerIsArmed()
    {
        await ArmBreaker();
        await Then_BreakerIsArmed();
    }

    [TestMethod]
    public async Task Given_ArmBreaker_When_ClearBreaker_Then_BreakerIsCleared()
    {
        await ArmBreaker();
        await ClearBreaker();
        await Then_BreakerIsCleared();
    }

    [TestMethod]
    public async Task Given_FaultBreaker_Then_BreakerIsFaulted()
    {
        await FaultBreaker();
        await Then_BreakerIsFaulted();
    }

    [TestMethod]
    public async Task Given_FaultBreaker_When_ClearBreaker_BreakerIsCleared()
    {
        await FaultBreaker();
        await ClearBreaker();
        await Then_BreakerIsCleared();
    }

    [TestMethod]
    public async Task Given_ArmBreaker_When_ClearBreaker_Then_BreakerDoesNotBecomeFaulted()
    {
        // when the breaker gets armed, if there is no success before
        // TimeToFaulted, it will go to a Faulted state.
        // if a success happens before then, the after TimeToFaulted,
        // it should not go into a the faulted state.
        await ArmBreaker();
        await Task.Delay(_config.TimeToFaulted / 2);
        await ClearBreaker();
        // wait for the timer that would have progressed to faulted
        await Task.Delay(_config.TimeToFaulted / 2);
        await Then_BreakerIsCleared();
    }

    [TestMethod]
    public async Task Given_MultipleFaults_Then_TimeToFaultIsFromFirstTime()
    {
        await ArmBreaker();
        await Task.Delay(_config.TimeToFaulted / 2);
        await ArmBreaker();
        await Task.Delay(_config.TimeToFaulted / 2);
        await Then_BreakerIsFaulted();
    }

    /// <summary>
    /// Puts the breaker into a faulted state.
    /// </summary>
    private async Task FaultBreaker()
    {
        // when there is a failure, the breaker is armed.
        // when there is no success for the configured 
        // TimeToFaulted then the breaker becomes faulted.
        await MeasureFailure();
        await Task.Delay(_config.TimeToFaulted);
    }

    /// <summary>
    /// Puts the breaker into a Cleared State.
    /// </summary>
    /// <returns></returns>
    private async Task ClearBreaker() => await _breaker.Guard(() => Task.CompletedTask);

    /// <summary>
    /// Puts the breaker into an Armed State
    /// </summary>
    private async Task ArmBreaker()
    {
        await Assert.ThrowsExactlyAsync<TestException>(
            () => _breaker.Guard(() => throw new TestException()));
    }

    /// <summary>
    /// Asserts that the breaker is an armed state
    /// by checking that guard calls are delayed the appropriate amount.
    /// </summary>
    private async Task Then_BreakerIsArmed()
    {
        var elapsed = await MeasureSuccess();
        Assert.IsTrue(elapsed.Add(Tolerance) >= _config.ArmedDelay,
            $"Breaker is not Armed. {elapsed} > {_config.ArmedDelay}");
        Assert.IsTrue(elapsed.Subtract(Tolerance) <= _config.FaultedDelay,
            $"Breaker is not Armed. {elapsed} > {_config.FaultedDelay}");
    }

    /// <summary>
    /// Asserts that the breaker is in Cleared State
    /// by checking that guard calls are not delayed.
    /// </summary>
    private async Task Then_BreakerIsCleared()
    {
        var elapsed = await MeasureSuccess();
        Assert.IsTrue(elapsed.Subtract(Tolerance) <= _config.ArmedDelay,
            $"Breaker is not Cleared. {elapsed} > {_config.ArmedDelay}");
    }

    /// <summary>
    /// Asserts that the breaker is in a faulted state
    /// by checking that guard calls are delayed by the larger amount.
    /// </summary>
    private async Task Then_BreakerIsFaulted()
    {
        // if a breaker is faulted, then any call
        // to the guard will take more tha the configured
        // faulted delay.
        var elapsed = await MeasureSuccess();
        Assert.IsTrue(elapsed.Add(Tolerance) >= _config.FaultedDelay,
            $"Breaker is not Faulted. {elapsed} < {_config.FaultedDelay}");
    }

    /// <summary>
    /// Call the guard and cause a fault.
    /// </summary>
    /// <returns>The elapsed time for the call.</returns>
    private async Task<TimeSpan> MeasureFailure()
    {
        var s = new Stopwatch();
        s.Restart();
        await ArmBreaker();
        s.Stop();
        return s.Elapsed;
    }

    /// <summary>
    /// Call the guard, and do not cause fault.
    /// </summary>
    /// <returns>The elapsed time for the call./returns>
    private async Task<TimeSpan> MeasureSuccess()
    {
        var s = new Stopwatch();
        s.Start();
        await ClearBreaker();
        s.Stop();
        return s.Elapsed;
    }
}
