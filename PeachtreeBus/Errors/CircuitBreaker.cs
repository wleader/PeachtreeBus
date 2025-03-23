using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Errors;

public interface ICircuitBreaker : IDisposable
{
    CircuitBreakerConfiguraton Configuration { get; }
    Task Guard(Func<CancellationToken, Task> asyncAction, CancellationToken cancellationToken);
    Task<T> Guard<T>(Func<CancellationToken, Task<T>> asyncFunction, CancellationToken cancellationToken);
    Task Guard(Func<Task> asyncAction);
    Task<T> Guard<T>(Func<Task<T>> asyncFunction);
}


/// <summary>
/// Introduces Delays when the Guarded operations throw exceptions.
/// </summary>
/// <remarks>
/// A breaker has three states: Cleared, Armed, and Faulted.
/// While in a Cleared state the Guard methods introduce no delay.
/// While in a Armed or Faulted state, Guard methods introduce a configured delay.
/// When in a Cleared state if a Guard method catches an exception,
/// the breaker changes to the Armed State.
/// When in the Armed State or Faulted State if a Guard method succeeds
/// without an exception the breaker changes to the Cleared State.
/// When in the Armed State if the breaker does not return to the Cleared state
/// after a configured amount of time it will change to the Faulted State.
/// </remarks>
public class CircuitBreaker : ICircuitBreaker
{
    private enum State
    {
        Clear,
        Armed,
        Faulted,
    }

    private readonly ILogger<CircuitBreaker> _log;
    private readonly Timer _triggerTimer;
    private long _failures = 0;
    private volatile State _state = State.Clear;

    public CircuitBreaker(
        ILogger<CircuitBreaker> log,
        CircuitBreakerConfiguraton configuraton)
    {
        _log = log;
        Configuration = configuraton;
        _triggerTimer = new(ArmedTimeout);
    }

    public CircuitBreakerConfiguraton Configuration { get; }

    public async Task Guard(Func<CancellationToken, Task> asyncAction, CancellationToken cancellationToken)
    {
        await DelayAsNeeded(cancellationToken);
        try
        {
            await asyncAction(cancellationToken);
            Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            Failure();
            throw;
        }
    }

    public Task Guard(Func<Task> asyncAction) => Guard((c) => asyncAction(), default);

    public Task<T> Guard<T>(Func<Task<T>> asyncFunction) => Guard((c) => asyncFunction(), default);

    public async Task<T> Guard<T>(Func<CancellationToken, Task<T>> asyncFunction, CancellationToken cancellationToken)
    {
        T? result = default;
        await Guard(async (CancellationToken t) =>
            result = await asyncFunction(t),
            cancellationToken);
        return result!;
    }

    private Task DelayAsNeeded(CancellationToken cancellationToken = default)
    {
        var delay = _state switch
        {
            State.Armed => Configuration.ArmedDelay,
            State.Faulted => Configuration.FaultedDelay,
            _ => TimeSpan.Zero,
        };
        return Task.Delay(delay, cancellationToken);
    }

    private void Success()
    {
        var priorValue = Interlocked.Exchange(ref _failures, 0);
        if (priorValue == 0)
            return;
        // cancel any pending timer.
        _triggerTimer.Change(Timeout.Infinite, Timeout.Infinite);
        _state = State.Clear;
        _log.CircuitBreaker_Cleared(Configuration.FriendlyName);
    }

    private void Failure()
    {
        var newcount = Interlocked.Increment(ref _failures);
        if (newcount > 1)
            return;
        _state = State.Armed;
        _triggerTimer.Change(Configuration.TimeToFaulted, Timeout.InfiniteTimeSpan);
        _log.CircuitBreaker_Armed(Configuration.FriendlyName);
    }

    private void ArmedTimeout(object? state)
    {
        // the breaker was armed, but there was
        // no success to clear the timer
        // proceed to faulted state.
        _state = State.Faulted;
        _log.CircuitBreaker_Faulted(Configuration.FriendlyName);
    }

    public void Dispose()
    {
        _triggerTimer.Dispose();
        GC.SuppressFinalize(this);
    }
}
