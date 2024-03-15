using System;
using System.Collections.Generic;

namespace PeachtreeBus
{
    /// <summary>
    /// Defines an interface for a Dependency Injection Scope.
    /// Can create instances of objects from the DI Container
    /// that are in the current scope as the IWrappedScope itself.
    /// </summary>
    public interface IWrappedScope : IDisposable

    {
        T GetInstance<T>() where T : class;
        object GetInstance(Type t);
        IEnumerable<T> GetAllInstances<T>() where T : class;
    }

    /// <summary>
    /// Defines an interface for creating an IWrappedScope
    /// </summary>
    public interface IWrappedScopeFactory
    {
        IWrappedScope Create();
    }
}
