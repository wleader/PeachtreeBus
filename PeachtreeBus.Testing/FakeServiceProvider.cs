using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PeachtreeBus.Testing;

/// <summary>
/// Wraps a Mock IServiceProvider and adds methods to make setting up the service provider easier.
/// </summary>
public class FakeServiceProvider
{
    // Represents a 'registration' with the IServiceProvider.
    [ExcludeFromCodeCoverage(Justification = "Generated Code")]
    private readonly record struct Registration(Func<object> GetFunction, object? Mock, Action? Callback, Action? Throws);

    // what has been 'registered' with the IServiceProvider.
    private readonly Dictionary<Type, Registration> _registry = [];

    public FakeServiceProvider() => Reset();

    /// <summary>
    /// Restores the Fake to its initial state when it was constructed.
    /// </summary>
    public void Reset()
    {
        _registry.Clear();
        AddMock(Mock);
        Mock.Reset();
        Mock.Setup(x => x.GetService(It.IsAny<Type>()))
            .Returns((Type t) => GetInstance(t));
    }

    /// <summary>
    /// The IServiceProvider that can be provided to code under test.
    /// </summary>
    public IServiceProvider Object { get => Mock.Object; }

    /// <summary>
    /// The internal Mock for the IServiceProvider.
    /// </summary>
    public Mock<IServiceProvider> Mock { get; } = new();

    private void Add<T>(Registration service) => _registry[typeof(T)] = service;

    /// <summary>
    /// Registers a mock with the IServiceProvider.
    /// Will replace any previous registration of type T.
    /// </summary>
    /// <typeparam name="T">The type of the mock service.</typeparam>
    /// <param name="mock">An existing Mock, or if null, a new mock will be created.</param>
    /// <param name="callback">An action that will be invoked when the IServiceProvide provides an instance of the registered service.</param>
    /// <returns>The provided or created Mock</returns>
    public Mock<T> AddMock<T>(Mock<T>? mock = null, Action? callback = null) where T : class
    {
        mock ??= new();
        Add<T>(new(() => mock.Object, mock, callback, null));
        return mock;
    }

    /// <summary>
    /// Registers an implementation of type T.
    /// Will replace any previous registration of type T.
    /// </summary>
    /// <typeparam name="T">The service type being registered.</typeparam>
    /// <param name="instance">An instance of type T to register.</param>
    /// <param name="callback">An action that will be invoked when the IServiceProvide provides an instance of the registered service.</param>
    public void Add<T>(T instance, Action? callback = null) where T : class =>
        Add<T>(new(() => instance, null, callback, null));

    /// <summary>
    /// Registers a function that returns and Implementation  of type T.
    /// </summary>
    /// <typeparam name="T">The service type being registered.</typeparam>
    /// <param name="func">A function called when the IServiceProvider gets a service of type T.
    /// This is called each time the service is provided.</param>
    /// <param name="callback">An action that will be invoked when the IServiceProvide provides an instance of the registered service.</param>
    public void Add<T>(Func<T> func, Action? callback = null) where T : class =>
        Add<T>(new(func, null, callback, null));

    private Registration? TryGetService(Type t) =>
        _registry.TryGetValue(t, out var service) ? service : default;

    private object? GetInstance(Type t) =>
        GetInstance(TryGetService(t));

    private static object? GetInstance(Registration? match)
    {
        match?.Throws?.Invoke();
        match?.Callback?.Invoke();
        return match?.GetFunction?.Invoke();
    }

    /// <summary>
    /// Gets the instance of a registered service.
    /// </summary>
    /// <typeparam name="T">The type of service to get.</typeparam>
    /// <returns>The instance of the registerd service.</returns>
    /// <exception cref="FakeServiceProvderException">Thrown if no service of type T was registered.</exception>
    public T GetRegistered<T>() where T : class =>
        TryGetService(typeof(T))?.GetFunction?.Invoke() as T
            ?? throw new FakeServiceProvderException($"A mock for type {typeof(T)} has not been added.");

    /// <summary>
    /// Returns the Mock of a registered service.
    /// </summary>
    /// <typeparam name="T">The type of the service that is mocked.</typeparam>
    /// <returns>The Mock that was registered.</returns>
    /// <exception cref="FakeServiceProvderException">Thrown if the type is not registered or if the registration is not a mock.</exception>
    public Mock<T> GetMock<T>() where T : class =>
        TryGetService(typeof(T))?.Mock as Mock<T>
            ?? throw new FakeServiceProvderException($"A mock for type {typeof(T)} has not been added.");

    /// <summary>
    /// Verifies that a service was provided a specific number of times.
    /// </summary>
    /// <typeparam name="T">The type of the service to verify.</typeparam>
    /// <param name="times">The expected number of times the service should have been provided.</param>
    public void VerifyGetService<T>(int times = 1) =>
        Mock.Verify(x => x.GetService(typeof(T)), Times.Exactly(times));

    /// <summary>
    /// Configures the service provider to throw an exception 
    /// </summary>
    /// <typeparam name="TInterface">The type of the service to configure.</typeparam>
    /// <typeparam name="TException">The type of the exception to throw.</typeparam>
    /// <param name="callback">A call back that will be called when the service is provided.</param>
    /// <param name="exception">An exception to throw. If null an exception will be thrown.</param>
    public void SetupThrow<TInterface, TException>(Action? callback = null, TException? exception = null)
        where TException : Exception, new()
        where TInterface : class =>
        Add<TInterface>(new(() => null!, null, callback, () => throw exception ?? new TException()));
}

public class FakeServiceProvderException(string? message) : Exception(message);
