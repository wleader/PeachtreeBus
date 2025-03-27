using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace PeachtreeBus.Tests.Telemetry;

public class ActivityFixtureBase(ActivitySource targetSource)
{
    protected TestActivityListener _listener = default!;
    private readonly ActivitySource targetSource = targetSource;

    [TestInitialize]
    public void Initialize()
    {
        _listener = new(targetSource);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _listener?.Dispose();
    }
}
