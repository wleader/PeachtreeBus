using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace PeachtreeBus.MicrosoftDependencyInjection;

public class MSDIWrappedScope : IWrappedScope
{
    public AsyncServiceScope? Scope { get; set; }

    public void Dispose()
    {
        Scope?.Dispose();
        GC.SuppressFinalize(this);
    }

    public IEnumerable<T> GetAllInstances<T>() where T : class => GetInstance<IEnumerable<T>>();

    public T GetInstance<T>() where T : class =>
        Scope.HasValue
            ? Scope.Value.ServiceProvider.GetRequiredService<T>()
            : throw new InvalidOperationException("Scope must be set before getting instances.");

    public object GetInstance(Type t) =>
        Scope.HasValue
            ? Scope.Value.ServiceProvider.GetRequiredService(t)
            : throw new InvalidOperationException("Scope must be set before getting instances.");

    public object? GetService(Type serviceType) => GetInstance(serviceType);
}
