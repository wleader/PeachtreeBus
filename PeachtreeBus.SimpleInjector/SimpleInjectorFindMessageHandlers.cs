using SimpleInjector;
using System.Collections.Generic;

namespace PeachtreeBus.SimpleInjector
{
    public class SimpleInjectorFindMessageHandlers : IFindMessageHandlers
    {
        private readonly Container _container;
        public SimpleInjectorFindMessageHandlers(Container container)
        {
            _container = container;
        }

        public IEnumerable<IHandleMessage<T>> FindHandlers<T>() where T : IMessage
        {
            return _container.GetAllInstances<IHandleMessage<T>>();
        }
    }
}
