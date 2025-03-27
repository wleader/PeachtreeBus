using Microsoft.VisualStudio.TestTools.UnitTesting;
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

    public Activity ExpectOneCompleteActivity()
    {
        Assert.AreEqual(0, Started.Count, "There are incomplete activities.");
        Assert.AreEqual(1, Stopped.Count, "There is not exactly 1 compeleted activity.");
        return Stopped[0];
    }

    public void Dispose()
    {
        _listener?.Dispose();
        GC.SuppressFinalize(this);
    }
}
