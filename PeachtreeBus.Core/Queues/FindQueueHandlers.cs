using PeachtreeBus.Exceptions;
using System.Collections.Generic;

namespace PeachtreeBus.Queues;

/// <summary>
/// an implementation of IFindQueueHandlers that gets the handlers from a SimpleInjector container.
/// </summary>
public class FindQueueHandlers(
    IWrappedScope scope)
    : IFindQueueHandlers
{
    private readonly IWrappedScope _scope = scope;

    public IEnumerable<IHandleQueueMessage<T>> FindHandlers<T>() where T : IQueueMessage
    {
        // create the handlers in the current scope.
        // this is because when multiple threads are handling messages, each thread needs 
        // to have its own instance of the handler object, so that mulitple threads do not
        // intefere with each other.
        return _scope.GetAllInstances<IHandleQueueMessage<T>>()
            ?? throw new IncorrectImplementationException(
                "An IWrappedScope must  not return null, when GetAllInstances is called.",
                _scope.GetType(),
                typeof(IWrappedScope));
    }
}
