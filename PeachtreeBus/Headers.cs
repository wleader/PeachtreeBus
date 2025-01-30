using System;
using System.Collections.Generic;

namespace PeachtreeBus;

public class UserHeaders : Dictionary<string, string>;

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
        MessageClass = GetMessageClassString(userMessageType);
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

    public static string GetMessageClassString(Type type)
    {
        return type.FullName + ", " + type.Assembly.GetName().Name;
    }
}

