using System;

namespace PeachtreeBus.Errors;

/// <summary>
/// Used to communicate back to the reader classes what to do with
/// a message after an unhandled exception in a message handler.
/// </summary>
public readonly record struct RetryResult
{
    /// <summary>
    /// If True, the message will be retried.
    /// </summary>
    public bool ShouldRetry { get; }

    /// <summary>
    /// When ShouldRetry is true, then this valuecontrols how 
    /// long to wait before retrying the message.
    /// </summary>
    public TimeSpan Delay { get; }

    public RetryResult(bool shouldRetry, TimeSpan delay)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(delay.Ticks, nameof(delay) + ".Ticks");
        ShouldRetry = shouldRetry;
        Delay = delay;
    }
}
