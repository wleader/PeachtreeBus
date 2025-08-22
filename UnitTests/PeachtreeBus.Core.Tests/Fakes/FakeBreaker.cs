using Moq;
using PeachtreeBus.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Fakes;

public class FakeBreaker : ICircuitBreaker
{
    public CircuitBreakerConfiguraton Configuration => throw new NotImplementedException();
    public CircuitBreakerState State => throw new NotImplementedException();
    public Task Guard(Func<CancellationToken, Task> asyncAction, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<T> Guard<T>(Func<CancellationToken, Task<T>> asyncFunction, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task Guard(Func<Task> asyncAction) => throw new NotImplementedException();
    public Task<T> Guard<T>(Func<Task<T>> asyncFunction) => throw new NotImplementedException();
    public void Guard(Action action) => action.Invoke();
}

public class FakeBreakerProvider : ICircuitBreakerProvider
{
    private FakeBreaker _breaker = new();

    public BreakerKey BusDataConnectionKey => new();

    public ICircuitBreaker GetBreaker(BreakerKey breakerKey) => _breaker;
}
