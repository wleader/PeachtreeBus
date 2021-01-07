using System;
using System.Collections.Generic;

namespace PeachtreeBus
{
    /// <summary>
    /// Defines an interface for a Dependency Injection Scope Manager.
    /// </summary>
    public interface IScopeManager
    {
        T GetInstance<T>() where T : class;
        object GetInstance(Type t);
        IEnumerable<T> GetAllInstances<T>() where T : class;
    }
}
