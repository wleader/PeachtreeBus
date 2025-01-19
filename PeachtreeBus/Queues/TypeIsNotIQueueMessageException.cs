using System;

namespace PeachtreeBus.Queues;

public class TypeIsNotIQueueMessageException : MissingInterfaceException
{
    internal TypeIsNotIQueueMessageException(Type classType) : base(classType, typeof(IQueueMessage)) { }

    public static void ThrowIfMissingInterface(Type type)
    {
        if (!typeof(IQueueMessage).IsAssignableFrom(type))
            throw new TypeIsNotIQueueMessageException(type);
    }
}
