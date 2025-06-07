using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PeachtreeBus.SimpleInjector
{
    /// <summary>
    /// An Implementation of IWrappedScopeFactory that uses Simple Injector
    /// </summary>
    public class SimpleInjectorScopeFactory(Container container) : IWrappedScopeFactory
    {
        private readonly Container _container = container;

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

        public IEnumerable<Type> GetImplementations<TInterface>()
        {
            return _container.GetRootRegistrations()
                .Where(ip => ip.ImplementationType.GetInterfaces().Contains(typeof(TInterface)))
                .Select(ip => ip.ImplementationType);
        }
    }
}
