using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PeachtreeBus.SimpleInjector
{
    /// <summary>
    /// An implementation of IWrappedScope for Simple Injector.
    /// </summary>
    public class SimpleInjectorScope : IWrappedScope
    {
        public Scope? Scope { get; set; }

        public void Dispose()
        {
            Scope?.Dispose();
            GC.SuppressFinalize(this);
        }

        public IEnumerable<T> GetAllInstances<T>() where T : class
        {
            if (Scope == null) throw new InvalidOperationException("Scope must be set before getting instances.");
            var instances = Scope.Container!.GetAllInstances<T>();
            return instances.Select(i => (T)Scope.GetInstance(i.GetType()));
        }

        public T GetInstance<T>() where T : class
        {
            if (Scope == null) throw new InvalidOperationException("Scope must be set before getting instances.");
            return Scope.GetInstance<T>();
        }

        public object GetInstance(Type t)
        {
            if (Scope == null) throw new InvalidOperationException("Scope must be set before getting instances.");
            return Scope.GetInstance(t);
        }

        public object? GetService(Type serviceType) => GetInstance(serviceType);
    }
}
