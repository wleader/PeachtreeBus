using System;
using System.Linq;

namespace PeachtreeBus.Sagas;

public static class SagaTypeExtensions
{
    /// <summary>
    /// Returns True if the Type inherits Saga<>.
    /// </summary>
    public static bool IsSubclassOfSaga(this Type? toCheck)
    {
        if (toCheck is null)
            return false;

        do
        {
            var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
            if (typeof(Saga<>) == cur)
                return true;

            toCheck = toCheck.BaseType;
        }
        while (toCheck is not null);
        return false;
    }

    /// <summary>
    /// returns true if the handler method is IHandleSagaStartMessage
    /// </summary>
    public static bool IsSagaStartHandler(this Type handlerType, Type messageType)
    {
        var handlerInterfaces = handlerType.GetInterfaces();
        var @interface = typeof(IHandleSagaStartMessage<>).MakeGenericType(messageType);
        return handlerInterfaces.Any(i => i == @interface);
    }
}
