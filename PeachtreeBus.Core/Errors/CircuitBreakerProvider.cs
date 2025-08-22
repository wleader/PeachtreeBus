using Microsoft.Extensions.Logging;
using PeachtreeBus.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeachtreeBus.Errors;

public enum BreakerType
{
    DatabaseConnection,
}

[ExcludeFromCodeCoverage(Justification = "Generated Code")]
public readonly record struct BreakerKey(BreakerType BreakerType, string Key);

public interface ICircuitBreakerProvider
{
    public BreakerKey BusDataConnectionKey { get; }

    ICircuitBreaker GetBreaker(BreakerKey breakerKey);
}

public class CircuitBreakerProvider(
    IDelayFactory delayFactory,
    ILogger<CircuitBreaker> breakerLogger,
    IBusConfiguration busConfiguration,
    ICircuitBreakerConfigurationProvider configProvider,
    ISystemClock clock)
    : ICircuitBreakerProvider
{
    private ConcurrentDictionary<BreakerKey, ICircuitBreaker> _breakers = [];

    public BreakerKey BusDataConnectionKey { get; } = new(BreakerType.DatabaseConnection, busConfiguration.ConnectionString);

    public ICircuitBreaker GetBreaker(BreakerKey key) => _breakers.GetOrAdd(key, CreateBreaker);

    private ICircuitBreaker CreateBreaker(BreakerKey key) =>
        new CircuitBreaker(delayFactory, breakerLogger, clock, configProvider.Get(key));
}

