using System;

namespace PeachtreeBus.Tasks;

public interface INextRunTracker
{
    bool WorkDue { get; }
    void WorkDone();
}

public abstract class NextRunTracker(
    ISystemClock clock,
    TimeSpan? interval = null)
    : INextRunTracker
{
    private readonly ISystemClock _clock = clock;
    private readonly bool Configured = interval.HasValue;
    private readonly TimeSpan _interval = interval ?? TimeSpan.MaxValue;
    private DateTime NextUpdate = DateTime.MinValue;
    public bool WorkDue => Configured && (NextUpdate < _clock.UtcNow);
    public void WorkDone() => NextUpdate = _clock.UtcNow.Add(_interval);
}
