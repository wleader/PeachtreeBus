using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Errors;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Errors;

[TestClass]
public class CircuitBreakerFixture
{
    private CircuitBreaker _breaker = default!;
    private readonly Mock<ILogger<CircuitBreaker>> _log = new();
    private readonly Mock<IDelayFactory> _delayFactory = new();
    private readonly TaskCompletionSource _progressToFaulted = new();

    private readonly CircuitBreakerConfiguraton _config =
        new("DefaultBreakerKey", "Default Breaker")
        {
            ArmedDelay = TimeSpan.FromMilliseconds(10),
            FaultedDelay = TimeSpan.FromMilliseconds(50),
            // TimeToFaulted must be different from the other two or the tests will break.
            TimeToFaulted = TimeSpan.FromMilliseconds(100),
        };

    // Use an exception defined here, because then
    // the guard code must be catching System.Exception.
    private class TestException : Exception;


    [TestInitialize]
    public void Initialize()
    {
        _log.Reset();
        _delayFactory.Reset();

        _delayFactory.Setup(d => d.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns((TimeSpan s, CancellationToken _) =>
                s == _config.TimeToFaulted
                    ? _progressToFaulted.Task
                    : Task.CompletedTask);

        // because the _delay factory above matches time to faulted, if its not
        // different from the other two, the tests are broken.
        Assert.AreNotEqual(_config.TimeToFaulted, _config.FaultedDelay, "Invalid Test Setup");
        Assert.AreNotEqual(_config.TimeToFaulted, _config.ArmedDelay, "Invalid Test Setup");

        _breaker = new(
            _delayFactory.Object,
            _log.Object,
            _config);
    }

    [TestMethod]
    public async Task Given_FaultBreaker_When_Guard_Then_DelayFactoryIsCalledCorrectly()
    {
        var tokenSource = new CancellationTokenSource();
        await FaultBreaker();
        await _breaker.Guard(Cancellable, tokenSource.Token);
        _delayFactory.Verify(f => f.Delay(_config.FaultedDelay, tokenSource.Token), Times.Once);
    }

    [TestMethod]
    public async Task Given_ArmBreaker_When_Guard_Then_DelayFactoryIsCalledCorrectly()
    {
        var tokenSource = new CancellationTokenSource();
        await ArmBreaker();
        await _breaker.Guard(Cancellable, tokenSource.Token);
        _delayFactory.Verify(f => f.Delay(_config.ArmedDelay, tokenSource.Token), Times.Once);
    }

    [TestMethod]
    public async Task Given_ClearBreaker_When_Guard_Then_DelayFactoryIsCalledCorrectly()
    {
        var tokenSource = new CancellationTokenSource();
        await ClearBreaker();
        _delayFactory.Invocations.Clear();
        await _breaker.Guard(Cancellable, tokenSource.Token);
        _delayFactory.Verify(f => f.Delay(TimeSpan.Zero, tokenSource.Token), Times.Once);
    }

    [TestMethod]
    public async Task Given_BreakerCleared_When_Cancel_Then_BreakerIsCleared()
    {
        // a cancellation exception should not trigger a fault or arming.
        static Task Cancellable(CancellationToken token) { return Task.Delay(TimeSpan.FromMinutes(1), token); }
        var tokenSource = new CancellationTokenSource();
        var task = _breaker.Guard(Cancellable, tokenSource.Token);
        tokenSource.Cancel();
        await Assert.ThrowsExactlyAsync<TaskCanceledException>(() => task);
        Then_BreakerIsCleared();
    }

    [TestMethod]
    public async Task When_ReturnFailure_Then_Throws_And_BreakerIsArmed()
    {
        static Task<string> Fail() { throw new TestException(); }
        await Assert.ThrowsExactlyAsync<TestException>(
            () => _breaker.Guard(Fail));
        Then_BreakerIsArmed();
    }


    [TestMethod]
    public async Task When_CancellableReturnFailure_Then_Throws_And_BreakerIsArmed()
    {
        static Task<int> Fail(CancellationToken token) { throw new TestException(); }
        await Assert.ThrowsExactlyAsync<TestException>(
            () => _breaker.Guard((t) => Fail(t), default));
        Then_BreakerIsArmed();
    }

    [TestMethod]
    public async Task When_Failure_Then_Throws_And_BreakerIsArmed()
    {
        static Task Fail() { throw new TestException(); }
        await Assert.ThrowsExactlyAsync<TestException>(
            () => _breaker.Guard(Fail));
        Then_BreakerIsArmed();
    }

    [TestMethod]
    public async Task When_CancellableFailure_Then_Throws_And_BreakerIsArmed()
    {
        static Task Fail(CancellationToken token) { throw new TestException(); }
        await Assert.ThrowsExactlyAsync<TestException>(
            () => _breaker.Guard(Fail, default));
        Then_BreakerIsArmed();
    }

    [TestMethod]
    public async Task When_Succeed_Then_NoThrows_AndBreakerIsCleared()
    {
        static Task Succeed() { return Task.CompletedTask; }
        await _breaker.Guard(Succeed);
        Then_BreakerIsCleared();
    }

    [TestMethod]
    public async Task When_CancellableSucceed_Then_NoThrows_AndBreakerIsCleared()
    {
        static Task Succeed(CancellationToken token) { return Task.CompletedTask; }
        await _breaker.Guard(Succeed, default);
        Then_BreakerIsCleared();
    }

    [TestMethod]
    public async Task When_Succeed_Then_Result_AndBreakerIsCleared()
    {
        static Task<int> Succeed() { return Task.FromResult(42); }
        Assert.AreEqual(42, await _breaker.Guard(Succeed));
        Then_BreakerIsCleared();
    }

    [TestMethod]
    public async Task When_CancellableSucceed_Then_Result_AndBreakerIsCleared()
    {
        static Task<int> Succeed(CancellationToken token) { return Task.FromResult(3); }
        Assert.AreEqual(3, await _breaker.Guard(Succeed, default));
        Then_BreakerIsCleared();
    }

    [TestMethod]
    public async Task Given_ClearBreaker_Then_BreakerIsCleared()
    {
        await ClearBreaker();
        Then_BreakerIsCleared();
    }

    [TestMethod]
    public async Task Given_ArmBreaker_Then_BreakerIsArmed()
    {
        await ArmBreaker();
        Then_BreakerIsArmed();
    }

    [TestMethod]
    public async Task Given_ArmBreaker_When_ClearBreaker_Then_BreakerIsCleared()
    {
        await ArmBreaker();
        await ClearBreaker();
        Then_BreakerIsCleared();
    }

    [TestMethod]
    public async Task Given_FaultBreaker_Then_BreakerIsFaulted()
    {
        await FaultBreaker();
        Then_BreakerIsFaulted();
    }

    [TestMethod]
    public async Task Given_FaultBreaker_When_ClearBreaker_BreakerIsCleared()
    {
        await FaultBreaker();
        await ClearBreaker();
        Then_BreakerIsCleared();
    }

    [TestMethod]
    public async Task Given_ArmBreaker_When_ClearBreaker_Then_BreakerDoesNotBecomeFaulted()
    {
        // when the breaker gets armed, if there is no success before
        // TimeToFaulted, it will go to a Faulted state.
        // if a success happens before then, the after TimeToFaulted,
        // it should not go into a the faulted state.
        await ArmBreaker();
        await ClearBreaker();
        _progressToFaulted.SetResult();
        Then_BreakerIsCleared();
    }

    [TestMethod]
    public async Task Given_MultipleFaults_Then_TimeToFaultIsFromFirstTime()
    {
        await ArmBreaker();
        _delayFactory.Verify(f => f.Delay(_config.TimeToFaulted, CancellationToken.None), Times.Once);
        await ArmBreaker();
        _delayFactory.Verify(f => f.Delay(_config.TimeToFaulted, CancellationToken.None), Times.Once);
        Then_BreakerIsArmed();
        _progressToFaulted.SetResult();
        await Task.Delay(10);
        Then_BreakerIsFaulted();
    }

    private async Task FaultBreaker()
    {
        // when there is a failure, the breaker is armed.
        // when there is no success for the configured 
        // TimeToFaulted then the breaker becomes faulted.
        await ArmBreaker();
        _progressToFaulted.SetResult();
        await Task.Delay(10); // give time for the continutewith to set the state.
    }

    private async Task ClearBreaker() => await _breaker.Guard(() => Task.CompletedTask);

    private async Task ArmBreaker()
    {
        await Assert.ThrowsExactlyAsync<TestException>(
            () => _breaker.Guard(() => throw new TestException()));
    }

    private void Then_BreakerIsArmed() => Assert.AreEqual(CircuitBreakerState.Armed, _breaker.State);

    private void Then_BreakerIsCleared() => Assert.AreEqual(CircuitBreakerState.Clear, _breaker.State);

    private void Then_BreakerIsFaulted() => Assert.AreEqual(CircuitBreakerState.Faulted, _breaker.State);

    private static Task Cancellable(CancellationToken _) => Task.CompletedTask;
}
