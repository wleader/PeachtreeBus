using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PeachtreeBus.Tests.Telemetry;

public class TestActivityListener : IDisposable
{
    private readonly ActivityListener _listener;
    private readonly ActivitySource _source;

    public List<Activity> Started { get; } = [];
    public List<Activity> Stopped { get; } = [];

    static ActivitySamplingResult SampleAllData(
    ref ActivityCreationOptions<ActivityContext> options) =>
        ActivitySamplingResult.AllData;

    public TestActivityListener(ActivitySource source)
    {
        _source = source;
        _listener = new()
        {
            ShouldListenTo = s => ReferenceEquals(s, _source),
            Sample = SampleAllData,
            ActivityStarted = Started.Add,
            ActivityStopped = (a) =>
            {
                Started.Remove(a);
                Stopped.Add(a);
            },
        };
        ActivitySource.AddActivityListener(_listener);
    }

    public void Dispose()
    {
        _listener?.Dispose();
        GC.SuppressFinalize(this);
    }
}
