using Moq;
using System;

namespace PeachtreeBus.Testing;

/// <summary>
/// Wraps a Mock of IServiceProviderAccessor and configures it with the same behavior of 
/// PeachtreeBust.ServiceProviderAccessor.
/// </summary>
public class FakeServiceProviderAccessor
{
    /// <summary>
    /// The Wrapped Mock of IServiceProviderAccessor.
    /// </summary>
    public Mock<IServiceProviderAccessor> Mock { get; } = new();

    /// <summary>
    /// The mocked IServiceProviderAccessor.
    /// </summary>
    public IServiceProviderAccessor Object { get => Mock.Object; }

    /// <summary>
    /// The IServiceProvider that the IServiceProviderAccessor provides.
    /// </summary>
    public IServiceProvider? ServiceProvider => _existing ?? FakeProvider?.Object;

    /// <summary>
    /// The FakeServiceProvider if one was supplied to the constructor.
    /// </summary>
    public FakeServiceProvider? FakeProvider { get; }


    private IServiceProvider? _existing;

    /// <summary>
    /// Creates an instance of the FakeServiceProviderAccessor.
    /// </summary>
    /// <param name="provider">
    /// If not null, the FakeServiceProvider will be used to provide the IServiceProvider.
    /// If null, then the FakeServiceProviderAccessor will be in the uninitialzed state, which
    /// is useful for testing code that expects an uninitialized accessor.
    /// </param>
    public FakeServiceProviderAccessor(FakeServiceProvider? provider = null)
    {
        FakeProvider = provider;
        Reset();
    }

    /// <summary>
    /// Resets the Fake to its state after it was constructed.
    /// </summary>
    /// <param name="existingProvder">
    /// if provided, this IServiceProvider will be returned by the mocked IServiceProviderAccessor.
    /// </param>
    /// <exception cref="FakeServiceProviderAcccessorException"></exception>
    public void Reset(IServiceProvider? existingProvder = null)
    {
        FakeProvider?.Reset();
        _existing = existingProvder;

        Mock.Reset();

        Mock.Setup(x => x.UseExisting(It.IsAny<IServiceProvider>()))
            .Callback((IServiceProvider p) => _existing = p);

        Mock.SetupGet(x => x.ServiceProvider)
            .Returns(() => _existing
                ?? FakeProvider?.Object
                ?? throw new FakeServiceProviderAcccessorException("Attempt to get ServiceProvider before it is initialized."));

        Mock.SetupGet(x => x.IsConfigured)
            .Returns(() => (_existing ?? FakeProvider?.Object) is not null);
    }
}

public static class FakeServiceProviderAccessorExtensions
{
    private static FakeServiceProvider RequireProvider(this FakeServiceProviderAccessor accessor) =>
        accessor.FakeProvider
            ?? throw new InvalidOperationException("The accessor does not have a FakeProvider");

    /// <summary>
    /// Calls .AddMock on the FakeServiceProvider.
    /// Throws if the Accessor was not constructed with a FakeServiceProvider.
    /// </summary>
    public static Mock<T> AddMock<T>(this FakeServiceProviderAccessor accessor, Mock<T>? mock = null) where T : class =>
        accessor.RequireProvider().AddMock(mock);

    /// <summary>
    /// Calls .Add instance on the FakeServiceProvider.
    /// Throws if the Accessor was not constructed with a FakeServiceProvider.
    /// </summary>
    public static void Add<T>(this FakeServiceProviderAccessor accessor, T instance, Action? callback = null) where T : class =>
        accessor.RequireProvider().Add(instance, callback);

    /// <summary>
    /// Calls .Add Func on the FakeServiceProvider.
    /// Throws if the Accessor was not constructed with a FakeServiceProvider.
    /// </summary>
    public static void Add<T>(this FakeServiceProviderAccessor accessor, Func<T> func, Action? callback = null) where T : class =>
        accessor.RequireProvider().Add(func, callback);

    /// <summary>
    /// Calls .VerifyGetService Func on the FakeServiceProvider.
    /// Throws if the Accessor was not constructed with a FakeServiceProvider.
    /// </summary>
    public static void VerifyGetService<T>(this FakeServiceProviderAccessor accessor, int times) where T : class =>
        accessor.RequireProvider().VerifyGetService<T>(times);

    /// <summary>
    /// Calls .SetupThrow on the FakeServiceProvider.
    /// Throws if the Accessor was not constructed with a FakeServiceProvider.
    /// </summary>
    public static void SetupThrow<TInterface, TException>(this FakeServiceProviderAccessor accessor)
        where TException : Exception, new()
        where TInterface : class =>
        accessor.RequireProvider().SetupThrow<TInterface, TException>();
}

public class FakeServiceProviderAcccessorException(string? message) : Exception(message);
