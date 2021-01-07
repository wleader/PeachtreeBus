using PeachtreeBus;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeachtreeBus.SimpleInjector
{

    /// <summary>
    /// An implementation of IScopeManager for Simple Injector.
    /// Should be registered as a singleton.
    /// </summary>
    public class ScopeManager : IScopeManager
    {
        public static readonly IList<Scope> AllScopes = new List<Scope>();

        private readonly Scope _scope;

        public ScopeManager(Container container)
        {
            _scope = AsyncScopedLifestyle.BeginScope(container);
            lock(AllScopes)
            {
                AllScopes.Add(_scope);
            }
        }

        public IEnumerable<T> GetAllInstances<T>() where T : class
        {
            var instances = _scope.Container.GetAllInstances<T>();
            return instances.Select(i => (T)_scope.GetInstance(i.GetType()));
        }

        public T GetInstance<T>() where T : class
        {
            return _scope.GetInstance<T>();
        }

        public object GetInstance(Type t)
        {
            return _scope.GetInstance(t);
        }
    }

    public static partial class SimpleInjectorExtensions
    {

        /// <summary>
        /// Releases all scoped cached objects.
        /// </summary>
        /// <param name="container"></param>
        public static void DisposeAllScopes(this Container _)
        {
            lock (ScopeManager.AllScopes)
            {
                foreach (var sm in ScopeManager.AllScopes)
                {
                    sm.Dispose();
                }
            }
        }
    }
}
