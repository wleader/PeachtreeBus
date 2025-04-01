using System;
using System.Diagnostics;

namespace PeachtreeBus.Telemetry;

public abstract class BaseActivity : IDisposable
{
    protected Activity? _activity = default;

    public void Dispose()
    {
        _activity?.Dispose();
        GC.SuppressFinalize(this);
    }

    public void AddException(Exception exception)
    {
        _activity?
            .AddException(exception)
            .AddTag("error.type", exception.GetType().FullName);
    }
}
