using System;
using System.Runtime.CompilerServices;

namespace PeachtreeBus;

/// <summary>
/// Used to indicate to a user of the library that their custom implementation of something is doing something wrong.
/// </summary>
/// <param name="message"></param>
/// <param name="classType"></param>
/// <param name="interfaceType"></param>
public class IncorrectImplementationException(string message, Type classType, Type interfaceType)
    : PeachtreeBusException($"The class {classType} implements {interfaceType} incorrectly. {message}")
{
    public Type ClassType { get; } = classType;
    public Type InterfaceType { get; } = interfaceType;

    public static T ThrowIfNull<T>(
        T? parameter,
        Type classType,
        Type interfaceType,
        [CallerArgumentExpression(nameof(parameter))] string name = "Unnamed",
        string? message = null)
    {
        return parameter ?? throw new IncorrectImplementationException(
            $"{name} should never be null. {message}",
            classType,
            interfaceType);
    }
}


