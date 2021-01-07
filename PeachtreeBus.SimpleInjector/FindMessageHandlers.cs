using SimpleInjector;
using System.Collections.Generic;

namespace PeachtreeBus.SimpleInjector
{
    /// <summary>
    /// an implementation of IFindMessageHandlers that gets the handlers from a SimpleInjector container.
    /// </summary>
    public class FindMessageHandlers : IFindMessageHandlers
    {
        private readonly IScopeManager _scope;
        public FindMessageHandlers(IScopeManager scope)
        {
            _scope = scope;
        }

        public IEnumerable<IHandleMessage<T>> FindHandlers<T>() where T : IMessage
        {
            return _scope.GetAllInstances<IHandleMessage<T>>();
        }
    }
}
