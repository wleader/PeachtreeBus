using PeachtreeBus.Queues;
using System;

namespace PeachtreeBus.Exceptions;

public class TypeIsNotIQueueMessageException(Type classType)
    : MissingInterfaceException(classType, typeof(IQueueMessage))
{
    public static void ThrowIfMissingInterface(Type type)
    {
        if (!typeof(IQueueMessage).IsAssignableFrom(type))
            throw new TypeIsNotIQueueMessageException(type);
    }
}
