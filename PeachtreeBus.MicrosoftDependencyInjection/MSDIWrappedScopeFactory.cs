using Microsoft.Extensions.DependencyInjection;
using PeachtreeBus.Exceptions;
using System;

namespace PeachtreeBus.MicrosoftDependencyInjection;

public class MSDIWrappedScopeFactory(
    IServiceProvider serviceProvider)
    : IWrappedScopeFactory
{
    public IWrappedScope Create()
    {
        var nativeScope = serviceProvider.CreateAsyncScope();
        var result = nativeScope.ServiceProvider.GetService<IWrappedScope>();

        if (result is MSDIWrappedScope siWrappedScoped)
        {
            // put the native scope inside the wrapped scope,
            // so that it is available later when code needs to create
            // something from the scope.
            siWrappedScoped.Scope = nativeScope;
        }
        else
        {
            throw new MSDIWrappedScopeFactoryException("Could not get a PeachtreeBus.IWrappedScope of type PeachtreeBus.SimpleInjector.SimpleInjectorScope from the container. Did you replace the registration for IWrappedScope?");
        }
        return siWrappedScoped;
    }
}

public class MSDIWrappedScopeFactoryException : PeachtreeBusException
{
    internal MSDIWrappedScopeFactoryException(string message) : base(message) { }
}
