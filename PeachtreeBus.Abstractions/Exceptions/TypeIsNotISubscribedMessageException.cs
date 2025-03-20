using PeachtreeBus.Subscriptions;
using System;

namespace PeachtreeBus.Exceptions;

public class TypeIsNotISubscribedMessageException(Type classType)
    : MissingInterfaceException(classType, typeof(ISubscribedMessage))
{
    public static void ThrowIfMissingInterface(Type type)
    {
        if (!typeof(ISubscribedMessage).IsAssignableFrom(type))
            throw new TypeIsNotISubscribedMessageException(type);
    }
}
