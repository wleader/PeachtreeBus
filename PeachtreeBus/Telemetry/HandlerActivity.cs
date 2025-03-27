using System;
using System.Diagnostics;

namespace PeachtreeBus.Telemetry;

public class HandlerActivity : BaseActivity, IDisposable
{
    public HandlerActivity(Type handlerType, IIncomingContext context)
    {
        _activity = ActivitySources.User.StartActivity(
            $"peachtreebus.handler {handlerType.Name}",
            ActivityKind.Internal)
            ?.AddHandlerType(handlerType)
            ?.AddIncomingContext(context);
    }
}
