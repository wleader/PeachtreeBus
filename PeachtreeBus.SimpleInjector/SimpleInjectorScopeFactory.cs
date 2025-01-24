using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace PeachtreeBus.SimpleInjector
{
    /// <summary>
    /// An Implementation of IWrappedScopeFactory that uses Simple Injector
    /// </summary>
    public class SimpleInjectorScopeFactory : IWrappedScopeFactory
    {
        private readonly Container _container;

        public SimpleInjectorScopeFactory(Container container)
        {
            _container = container;
        }

        public IWrappedScope Create()
        {
            // start a new scope from the container.
            var scope = AsyncScopedLifestyle.BeginScope(_container);

            // create a wrapped scope inside the native scope.
            var wrapped = scope.GetInstance<IWrappedScope>();

            if (wrapped is SimpleInjectorScope siWrappedScoped)
            {
                // put the native scope inside the wrapped scope,
                // so that it is available later when code needs to create
                // something from the scope.
                siWrappedScoped.Scope = scope;
            }
            else
            {
                throw new SimpleInjectorScopeFactoryException("Could not get a PeachtreeBus.IWrappedScope of type PeachtreeBus.SimpleInjector.SimpleInjectorScope from the container. Did you replace the registration for IWrappedScope?");
            }
            return siWrappedScoped;
        }
    }
}
