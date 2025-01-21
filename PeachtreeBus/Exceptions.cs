using System;

namespace PeachtreeBus;

public abstract class PeachtreeBusException : Exception
{
    internal PeachtreeBusException(string message)
        : base(message)
    { }
}

public abstract class MissingInterfaceException : PeachtreeBusException
{
    public Type ClassType { get; }
    public Type InterfaceType { get; }

    internal MissingInterfaceException(Type classType, Type interfaceType)
        : base($"The type {classType} does not implement the required interface {interfaceType}.")
    {
        ClassType = classType;
        InterfaceType = interfaceType;
    }
}
