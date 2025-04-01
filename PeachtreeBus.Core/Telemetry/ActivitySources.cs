using System.Diagnostics;

namespace PeachtreeBus.Telemetry;

public static class ActivitySources
{
    // Core messaging activities.
    public static readonly ActivitySource Messaging = new("PeachtreeBus.Messaging", "0.11.0");

    // calls into user code.
    public static readonly ActivitySource User = new("PeachtreeBus.User", "0.11.0");

    // Stuff that peachtree bus developers could be interested in, but most users wouldn't
    public static readonly ActivitySource Internal = new("PeachtreeBus.Internal", "0.11.0");
    public static readonly ActivitySource DataAccess = new("PeachtreeBus.DataAccess", "0.11.0");
}
