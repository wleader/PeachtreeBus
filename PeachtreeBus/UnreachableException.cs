using System.Runtime.CompilerServices;

namespace PeachtreeBus;

/// <summary>
/// Typically used to supress nullablity warnings based on reflection results,
/// when the code knows that a particular type must have a certain member.
/// If this throws, the interface of something probably changed.
/// </summary>
/// <param name="message"></param>
public class UnreachableException(string message) : PeachtreeBusException(message)
{
    public static T ThrowIfNull<T>(T? parameter, [CallerArgumentExpression(nameof(parameter))] string name = "Unnamed", string? message = null)
    {
        return parameter ?? throw new UnreachableException($"{name} should never be null. {message}");
    }
}


