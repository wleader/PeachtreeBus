using System;
using System.Diagnostics;

namespace PeachtreeBus.Telemetry;

public class HandlerActivity(Type handlerType, IIncomingContext context) : IDisposable
{
    private readonly Activity? _activity = ActivitySources.User.StartActivity(
            $"peachtreebus.handler {handlerType.Name}",
            ActivityKind.Internal)
            ?.AddHandlerType(handlerType)
            ?.AddIncomingContext(context);

    public void Dispose()
    {
        _activity?.Dispose();
        GC.SuppressFinalize(this);
    }
}
