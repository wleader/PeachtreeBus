using PeachtreeBus.Exceptions;
using System.Linq;

namespace PeachtreeBus.Data;

public readonly record struct TableName
{
    public string Value { get; }
    public TableName(string value)
    {
        TableNameException.ThrowIfNotSafe(value);
        Value = value;
    }

    public override string ToString() => Value
        ?? throw new NotInitializedException(typeof(TableName));
}

public class TableNameException : PeachtreeBusException
{
    public const string AllowedCharacters = "abcdefghijklmnopqrstuvwxyz0123456789_";

    private TableNameException(string message) : base(message) { }

    public static string ThrowIfNotSafe(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new TableNameException($"A TableName cannot be null, an empty string, or entirely whitespace.");

        if (value.ToLower().Any(c => !AllowedCharacters.Contains(c)))
            throw new TableNameException($"A TableName can only contain the characters '{AllowedCharacters}'.");

        return value;
    }
}
