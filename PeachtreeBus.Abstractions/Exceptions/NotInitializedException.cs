using System;

namespace PeachtreeBus.Exceptions;

public class NotInitializedException(Type type)
    : PeachtreeBusException($"{type.Name} not initialized.")
{
    public Type Type { get; } = type;
}