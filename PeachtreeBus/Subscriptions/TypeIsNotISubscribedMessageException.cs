using System;

namespace PeachtreeBus.Subscriptions;

public class TypeIsNotISubscribedMessageException : MissingInterfaceException
{
    public TypeIsNotISubscribedMessageException(Type classType) : base(classType, typeof(ISubscribedMessage)) { }

    public static void ThrowIfMissingInterface(Type type)
    {
        if (!typeof(ISubscribedMessage).IsAssignableFrom(type))
            throw new TypeIsNotISubscribedMessageException(type);
    }
}
