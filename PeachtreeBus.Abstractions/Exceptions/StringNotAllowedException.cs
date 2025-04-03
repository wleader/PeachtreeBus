using System.Linq;

namespace PeachtreeBus.Exceptions;

public class StringNotAllowedException : PeachtreeBusException
{
    private StringNotAllowedException(string message) : base(message) { }

    public static string ThrowIfNotAllowed(string value, string typeName, string AllowedCharacters)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new StringNotAllowedException($"A {typeName} cannot be null, an empty string");

        if (value.ToLower().Any(c => !AllowedCharacters.Contains(c)))
            throw new StringNotAllowedException($"A {typeName} can only contain the characters '{AllowedCharacters}'.");

        return value;
    }
}
