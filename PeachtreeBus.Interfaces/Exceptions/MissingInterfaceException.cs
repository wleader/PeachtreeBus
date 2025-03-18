using System;

namespace PeachtreeBus.Exceptions;

public abstract class MissingInterfaceException(Type classType, Type interfaceType)
    : PeachtreeBusException($"The type {classType} does not implement the required interface {interfaceType}.")
{
    public Type ClassType { get; } = classType;
    public Type InterfaceType { get; } = interfaceType;
}
