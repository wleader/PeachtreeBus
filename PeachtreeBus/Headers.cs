using System;

namespace PeachtreeBus;

/// <summary>
/// Headers stored with the message.
/// </summary>
public class Headers
{
    public Headers()
    {
        MessageClass = string.Empty;
        UserHeaders = [];
    }

    public Headers(Type userMessageType, UserHeaders? userHeaders = null)
    {
        MessageClass = userMessageType.GetMessageClass();
        UserHeaders = userHeaders ?? [];
    }

    /// <summary>
    /// the type the message was serialized from and to deserialize it to.
    /// </summary>
    public string MessageClass { get; set; }

    /// <summary>
    /// Any exception details from a previous attempt to process the message.
    /// </summary>
    public string? ExceptionDetails { get; set; }

    /// <summary>
    /// A place for user code to store additional values along with the message.
    /// </summary>
    public UserHeaders UserHeaders { get; set; }

    public Diagnostics Diagnostics { get; set; } = new();
}

public class Diagnostics
{
    public string? TraceParent { get; set; } = null;
    public bool StartNewTraceOnReceive { get; set; } = false;
}
