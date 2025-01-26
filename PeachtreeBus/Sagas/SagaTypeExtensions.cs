using System;
using System.Linq;

namespace PeachtreeBus.Sagas
{
    public static class SagaTypeExtensions
    {
        /// <summary>
        /// Returns True if the Type inherits Saga<>.
        /// </summary>
        /// <param name="toCheck"></param>
        /// <returns></returns>
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
        /// returns true if the handler method can start a saga.
        /// </summary>
        /// <param name="handlerType"></param>
        /// <param name="messageType"></param>
        /// <returns></returns>
        public static bool IsSagaStartHandler(this Type handlerType, Type messageType)
        {
            var handlerInterfaces = handlerType.GetInterfaces();
            var handleSagaStartMessageInterface = typeof(IHandleSagaStartMessage<>).MakeGenericType(messageType);
            return handlerInterfaces.Any(i => i == handleSagaStartMessageInterface);
        }
    }
}
