using System;

namespace PeachtreeBus;

public abstract class PeachtreeBusException : Exception
{
    internal PeachtreeBusException(string message)
        : base(message)
    { }

    internal PeachtreeBusException(string message, Exception innerException)
        : base(message, innerException)
    { }

}

public abstract class MissingInterfaceException : PeachtreeBusException
{
    public Type ClassType { get; private set; }
    public Type InterfaceType { get; private set; }

    internal MissingInterfaceException(Type classType, Type interfaceType)
        : base($"The type {classType} does not implement the required interface {interfaceType}.")
    {
        ClassType = classType;
        InterfaceType = interfaceType;
    }
}
