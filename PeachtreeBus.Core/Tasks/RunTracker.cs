using System;

namespace PeachtreeBus.Tasks;

public interface ITracker
{
    bool ShouldStart { get; }
    void WorkDone();
    void Start();
}

public abstract class IntervalRunTracker(
    ISystemClock clock)
    : ITracker
{
    private readonly ISystemClock _clock = clock;
    private bool _started = false;
    public void Start() { _started = true; }
    public DateTime NextDue { get; private set; } = DateTime.MinValue;
    public bool ShouldStart => !_started && Interval.HasValue && (NextDue <= _clock.UtcNow);
    public void WorkDone()
    {
        _started = false;
        NextDue = Interval.HasValue
            ? _clock.UtcNow.Add(Interval.Value)
            : DateTime.MaxValue;
    }

    public abstract TimeSpan? Interval { get; }
}

public interface IAlwaysRunTracker : ITracker;

public class AlwaysRunTracker : IAlwaysRunTracker
{
    public bool ShouldStart => true;
    public void Start() { }
    public void WorkDone() { }
}
