using Microsoft.Extensions.Logging;
using PeachtreeBus.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Errors;

public enum CircuitBreakerState
{
    Clear,
    Armed,
    Faulted,
}

public interface ICircuitBreaker
{
    CircuitBreakerConfiguraton Configuration { get; }
    Task Guard(Func<CancellationToken, Task> asyncAction, CancellationToken cancellationToken);
    Task<T> Guard<T>(Func<CancellationToken, Task<T>> asyncFunction, CancellationToken cancellationToken);
    Task Guard(Func<Task> asyncAction);
    Task<T> Guard<T>(Func<Task<T>> asyncFunction);
    void Guard(Action action);
    public CircuitBreakerState State { get; }
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
public class CircuitBreaker(
    IDelayFactory delayFactory,
    ILogger<CircuitBreaker> log,
    ISystemClock clock,
    CircuitBreakerConfiguraton configuraton) : ICircuitBreaker
{
    private DateTime ArmedAt;

    public CircuitBreakerConfiguraton Configuration { get; } = configuraton;

    private volatile CircuitBreakerState _state = CircuitBreakerState.Clear;
    public CircuitBreakerState State => _state;

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

    public Task Guard(Func<Task> asyncAction) => Guard(async (c) => await asyncAction(), default);

    public Task<T> Guard<T>(Func<Task<T>> asyncFunction) => Guard(async (c) => await asyncFunction(), default);

    public async Task<T> Guard<T>(Func<CancellationToken, Task<T>> asyncFunction, CancellationToken cancellationToken)
    {
        T? result = default;
        async Task AsTask(CancellationToken token) => result = await asyncFunction(cancellationToken);
        await Guard(AsTask, cancellationToken);
        return result!;
    }

    public void Guard(Action action)
    {
        var t = new Task(action);
        var g = Guard((c) => t, default);
        t.Start();
        g.GetAwaiter().GetResult();
    } 

    private async Task DelayAsNeeded(CancellationToken cancellationToken = default)
    {
        var delay = _state switch
        {
            CircuitBreakerState.Armed => Configuration.ArmedDelay,
            CircuitBreakerState.Faulted => Configuration.FaultedDelay,
            _ => TimeSpan.Zero,
        };
        await delayFactory.Delay(delay, cancellationToken);
    }

    private void Success()
    {
        if (_state == CircuitBreakerState.Clear) return;
        _state = CircuitBreakerState.Clear;
        log.Cleared(Configuration.FriendlyName);
    }

    private void Failure()
    {
        // if its already in a faulted state there is nothing to do.
        if (_state == CircuitBreakerState.Faulted)
            return;

        // if its clear, then arm it.
        if (_state == CircuitBreakerState.Clear)
        {
            log.Armed(Configuration.FriendlyName);
            _state = CircuitBreakerState.Armed;
            ArmedAt = clock.UtcNow;
            return;
        }

        // the state must be armed.
        // if not enough time has passed, stay armed.
        if (ArmedAt.Add(Configuration.TimeToFaulted) > clock.UtcNow)
            return;

        // it is armed, and its been armed long enough to progress to faulted.
        _state = CircuitBreakerState.Faulted;
        log.Faulted(Configuration.FriendlyName);
    }
}
