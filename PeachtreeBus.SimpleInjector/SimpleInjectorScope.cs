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
        public Scope Scope { get; set; }

        public IEnumerable<T> GetAllInstances<T>() where T : class
        {
            var instances = Scope.Container.GetAllInstances<T>();
            return instances.Select(i => (T)Scope.GetInstance(i.GetType()));
        }

        public T GetInstance<T>() where T : class
        {
            return Scope.GetInstance<T>();
        }

        public object GetInstance(Type t)
        {
            return Scope.GetInstance(t);
        }
    }
}
