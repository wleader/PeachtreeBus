using SimpleInjector;
using System.Collections.Generic;

namespace PeachtreeBus.SimpleInjector
{
    /// <summary>
    /// an implementation of IFindMessageHandlers that gets the handlers from a SimpleInjector container.
    /// </summary>
    public class FindMessageHandlers : IFindMessageHandlers
    {
        private readonly Container _container;
        public FindMessageHandlers(Container container)
        {
            _container = container;
        }

        public IEnumerable<IHandleMessage<T>> FindHandlers<T>() where T : IMessage
        {
            return _container.GetAllInstances<IHandleMessage<T>>();
        }
    }
}
