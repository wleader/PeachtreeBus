using System;

namespace PeachtreeBus.Exceptions;

public class SerializerException(
    string? serializedData,
    Type type,
    string? message = null)
    : PeachtreeBusException(message ?? $"Failed to deserialize data as {type}.")
{
    public Type Type { get; } = type;
    public string? SerializedData { get; } = serializedData;
}
