using System;

namespace PeachtreeBus.Errors;

public class CircuitBreakerConfiguraton
{
    /// <summary>
    /// A Name for the breaker that can be logged.
    /// Use when the key contains something that should not be logged.
    /// </summary>
    public required string FriendlyName { get; init; }

    /// <summary>
    /// While armed, Guard calls are delayed by this much before 
    /// they continue.
    /// </summary>
    public TimeSpan ArmedDelay { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// While Faulted, Guard calls are delayed by this much before
    /// they continue.
    /// </summary>
    public TimeSpan FaultedDelay { get; init; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// How much time it takes to progress from Armed to Faulted.
    /// </summary>
    public TimeSpan TimeToFaulted { get; init; } = TimeSpan.FromSeconds(30);
}

public interface ICircuitBreakerConfigurationProvider
{
    CircuitBreakerConfiguraton Get(BreakerKey key);
}

public class CircuitBreakerConfigurationProvider(
    IBusConfiguration busConfiguration)
    : ICircuitBreakerConfigurationProvider
{
    private BreakerKey _busDatabaseConnectionKey = new(BreakerType.DatabaseConnection, busConfiguration.ConnectionString);

    private readonly CircuitBreakerConfiguraton BusDatabaseConnectionBreakerConfiguration = new()
    {
        FriendlyName = "Bus Database Connection",
        ArmedDelay = TimeSpan.FromSeconds(5),
        FaultedDelay = TimeSpan.FromSeconds(30),
        TimeToFaulted = TimeSpan.FromSeconds(30),
    };

    private readonly CircuitBreakerConfiguraton DefaultBreakerConfiguration = new()
    {
        FriendlyName = "Default Breaker",
        ArmedDelay = TimeSpan.FromSeconds(1),
        FaultedDelay = TimeSpan.FromSeconds(10),
        TimeToFaulted = TimeSpan.FromSeconds(30),
    };

    public CircuitBreakerConfiguraton Get(BreakerKey key) =>
        key == _busDatabaseConnectionKey
            ? BusDatabaseConnectionBreakerConfiguration
            : DefaultBreakerConfiguration;
}
