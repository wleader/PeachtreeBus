using PeachtreeBus;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeachtreeBus.SimpleInjector
{
    public class SimpleInjectorScopeManager : IScopeManager
    {
        private readonly Container _container;
        private readonly List<Scope> scopes = new List<Scope>();

        public SimpleInjectorScopeManager(Container container)
        {
            _container = container;
        }

        public void Begin()
        {
            lock(scopes)
            {
                scopes.Add(AsyncScopedLifestyle.BeginScope(_container));
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
}
