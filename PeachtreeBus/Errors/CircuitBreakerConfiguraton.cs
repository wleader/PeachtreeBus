using System;

namespace PeachtreeBus.Errors;

public class CircuitBreakerConfiguraton(string key, string? friendlyName)
{
    /// <summary>
    /// Uniquely identifies a Breaker. Useful for sharing breakers
    /// across threads and scopes.
    /// </summary>
    public string Key { get; } = key;

    /// <summary>
    /// A Name for the breaker that can be logged.
    /// Use when the key contains something that should not be logged.
    /// </summary>
    public string FriendlyName { get; } = friendlyName ?? key;

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
