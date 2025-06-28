using System;

namespace PeachtreeBus;

public interface IScopeFactory
{
    /// <summary>
    /// Starts a new Dependency Injection Scope,
    /// and returns a ServiceProvideAccessor for that scope.
    /// </summary>
    /// <returns></returns>
    IServiceProviderAccessor Create();
}

/// <summary>
/// Provides access to an IServiceProvider for the current scope.
/// </summary>
public interface IServiceProviderAccessor : IDisposable
{
    /// <summary>
    /// An IServiceProvier for the current scope.
    /// </summary>
    IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Allows setting the IServiceProvider.
    /// </summary>
    /// <remarks>
    /// If you don't know what you are doing, you probably don't need to use this.
    /// </remarks>
    /// <param name="serviceProvider"></param>
    void UseExisting(IServiceProvider serviceProvider);

    bool IsConfigured { get; }
}
