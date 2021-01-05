using PeachtreeBus;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeachtreeBus.SimpleInjector
{

    /// <summary>
    /// An implementation of IScopeManager for Simple Injector.
    /// Should be registered as a singleton.
    /// </summary>
    public class ScopeManager : IScopeManager
    {
        private readonly Container _container;
        private readonly List<Scope> scopes = new List<Scope>();

        public ScopeManager(Container container)
        {
            _container = container;
        }

        public void Begin()
        {
            lock(scopes)
            {
                scopes.Add(AsyncScopedLifestyle.BeginScope(_container) );
            }
        }

        public void DisposeAll()
        {
            lock(scopes)
            {
                foreach(var s in scopes)
                {
                    s.DisposeAsync().GetAwaiter().GetResult();
                }
            }
        }
    }

    public static partial class SimpleInjectorExtensions
    {

        /// <summary>
        /// Releases all scoped cached objects.
        /// </summary>
        /// <param name="container"></param>
        public static void DisposeAllScopes(this Container container)
        {
            var scopeManager = container.GetInstance<IScopeManager>();
            scopeManager.DisposeAll();
        }
    }
}
