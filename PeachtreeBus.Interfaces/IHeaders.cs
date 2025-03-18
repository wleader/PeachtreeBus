using System.Collections.Generic;

namespace PeachtreeBus;

/// <summary>
/// Storage for user headers.
/// Users can store anything they like with the message,
/// and these will be available when the message is handled.
/// </summary>
public class UserHeaders : Dictionary<string, string>;

/// <summary>
/// An interface for bus headers.
/// </summary>
public interface IHeaders
{
    /// <summary>
    /// Contains the Full name and Assembly Name of the type of the Message.
    /// </summary>
    public string MessageClass { get; }

    /// <summary>
    /// Contains the Exception .ToString()
    /// when there was an unhandled exception while processing the message.
    /// </summary>
    public string? ExceptionDetails { get; }

    /// <summary>
    /// Contains any supplied user headers.
    /// </summary>
    public UserHeaders UserHeaders { get; }
}