using Microsoft.Extensions.Logging;
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
    CircuitBreakerConfiguraton configuraton) : ICircuitBreaker
{
    private readonly ILogger<CircuitBreaker> _log = log;
    private readonly IDelayFactory _delayFactory = delayFactory;

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

    public Task Guard(Func<Task> asyncAction) => Guard((c) => asyncAction(), default);

    public Task<T> Guard<T>(Func<Task<T>> asyncFunction) => Guard((c) => asyncFunction(), default);

    public async Task<T> Guard<T>(Func<CancellationToken, Task<T>> asyncFunction, CancellationToken cancellationToken)
    {
        T? result = default;
        async Task AsTask(CancellationToken token) => result = await asyncFunction(cancellationToken);
        await Guard(AsTask, cancellationToken);
        return result!;
    }

    private Task DelayAsNeeded(CancellationToken cancellationToken = default)
    {
        var delay = _state switch
        {
            CircuitBreakerState.Armed => Configuration.ArmedDelay,
            CircuitBreakerState.Faulted => Configuration.FaultedDelay,
            _ => TimeSpan.Zero,
        };
        return _delayFactory.Delay(delay, cancellationToken);
    }

    private void Success()
    {
        if (_state == CircuitBreakerState.Clear) return;
        _state = CircuitBreakerState.Clear;
        _log.CircuitBreaker_Cleared(Configuration.FriendlyName);
    }

    private void Failure()
    {
        if (_state != CircuitBreakerState.Clear) return;
        _log.CircuitBreaker_Armed(Configuration.FriendlyName);
        _state = CircuitBreakerState.Armed;

        // wait for time to faulted.
        // if its still in an armed state, escalate to faulted.
        _ = _delayFactory
            .Delay(Configuration.TimeToFaulted, CancellationToken.None)
            .ContinueWith((_) =>
            {
                if (_state == CircuitBreakerState.Armed)
                {
                    _state = CircuitBreakerState.Faulted;
                    _log.CircuitBreaker_Faulted(Configuration.FriendlyName);
                }
            });
    }
}
