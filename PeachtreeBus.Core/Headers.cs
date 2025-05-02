using PeachtreeBus.ClassNames;
using System;

namespace PeachtreeBus;

/// <summary>
/// Headers stored with the message.
/// </summary>
public class Headers
{
    /// <summary>
    /// the type the message was serialized from and to deserialize it to.
    /// </summary>
    public ClassName MessageClass { get; set; } = ClassName.Default;

    /// <summary>
    /// Any exception details from a previous attempt to process the message.
    /// </summary>
    public string? ExceptionDetails { get; set; }

    /// <summary>
    /// A place for user code to store additional values along with the message.
    /// </summary>
    public UserHeaders UserHeaders { get; set; } = [];

    public Diagnostics Diagnostics { get; set; } = new();
}

public readonly record struct Diagnostics(
    string? TraceParent = null,
    bool StartNewTraceOnReceive = false);
