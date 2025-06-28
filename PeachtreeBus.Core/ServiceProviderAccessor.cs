using PeachtreeBus.Exceptions;
using System;
using System.Collections.Generic;

namespace PeachtreeBus;

public abstract class ServiceProviderAccessor<TScope> : IServiceProviderAccessor
    where TScope : IDisposable
{
    public IServiceProvider ServiceProvider =>
        _userSuppliedServiceProvider ??
        _factorySuppliedServiceProvider ??
            throw new ServiceProviderAccessorException(
                """
                The IServiceProvider has not been configured.
                Use IScopeFactory to create an new scope and IServiceProvider, or call the UseExisting method to set your own IServiceProvider
                """);

    protected TScope? _scope;
    private IServiceProvider? _userSuppliedServiceProvider;
    protected IServiceProvider? _factorySuppliedServiceProvider;

    public bool IsConfigured =>
        _factorySuppliedServiceProvider is not null ||
        _userSuppliedServiceProvider is not null;

    public void Initialize(TScope scope, IServiceProvider serviceProvider)
    {
        _scope = scope;
        _factorySuppliedServiceProvider = serviceProvider;
    }

    public void UseExisting(IServiceProvider serviceProvider)
    {
        _userSuppliedServiceProvider = serviceProvider;
    }

    public virtual void Dispose()
    {
        _factorySuppliedServiceProvider = null;
        _userSuppliedServiceProvider = null;
        _scope?.Dispose();
        GC.SuppressFinalize(this);
    }
}

public static class ServiceProviderAccessorExtensions
{
    public static IEnumerable<T> GetServices<T>(this IServiceProviderAccessor accessor) =>
        accessor.ServiceProvider.GetServices<T>();

    public static IEnumerable<T> GetServices<T>(this IServiceProvider serviceProvider) =>
        serviceProvider.GetService<IEnumerable<T>>() ?? [];

    public static T? GetService<T>(this IServiceProviderAccessor accessor) =>
        accessor.ServiceProvider.GetService<T>();

    public static T GetRequiredService<T>(this IServiceProviderAccessor accessor) =>
        accessor.ServiceProvider.GetRequiredService<T>();

    public static T? GetService<T>(this IServiceProvider serviceProvider) =>
        (T?)serviceProvider.GetService(typeof(T));

    public static T GetRequiredService<T>(this IServiceProvider serviceProvider) =>
        serviceProvider.GetService<T>()
             ?? throw new ServiceProviderAccessorException(
                $"The IServiceProvider did not provide an instance of {typeof(T)}");
}
