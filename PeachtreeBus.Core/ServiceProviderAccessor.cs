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
                Use IScopeFactory to create an IServiceProviderAccessor which will create a new Scope, or call the UseExisting method to provide your own scoped IServiceProvider.
                """);

    private bool _scopeSet;
    protected TScope? _scope;
    private IServiceProvider? _userSuppliedServiceProvider;
    protected IServiceProvider? _factorySuppliedServiceProvider;

    public bool IsConfigured =>
        _factorySuppliedServiceProvider is not null ||
        _userSuppliedServiceProvider is not null;

    public void Initialize(TScope scope, IServiceProvider serviceProvider)
    {
        _scopeSet = true;
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

        // if TScope is a struct, and Initialize hasn't been
        // called, then TScope might not be null, but also it might
        // be uninitialized, and therefore can't be disposed.
        // only attempt to dispose if Initialize was called.
        // This can be the case when using Microsoft.Extensions.DependencyInjection
        // AsyncServiceScope. There may be others in the future.
        if (_scopeSet)
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
             ?? throw new InvalidOperationException(
                $"No service for type '{typeof(T)}' has been registered.");
}
