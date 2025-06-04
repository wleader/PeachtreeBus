using PeachtreeBus.Exceptions;
using SimpleInjector;
using System;

namespace PeachtreeBus.SimpleInjector;

public class MissingRegistrationException(Type missingType, string? help)
    : PeachtreeBusException($"The type {missingType} is required but is not registered with the container. {help}")
{
    public Type MissingType { get; } = missingType;

    public static void ThrowIfNotRegistered<T>(Container container, string? help = null)
    {
        if (!container.IsRegistered<T>())
            throw new MissingRegistrationException(typeof(T), help);
    }
}
