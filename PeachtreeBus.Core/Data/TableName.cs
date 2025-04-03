using PeachtreeBus.Exceptions;
using PeachtreeBus.Sagas;

namespace PeachtreeBus.Data;

public readonly record struct TableName
{
    public const string AllowedCharacters = "abcdefghijklmnopqrstuvwxyz0123456789_";

    private readonly string _value;
    
    public string Value => _value
        ?? throw new NotInitializedException(typeof(TableName));

    public TableName(string value)
    {
        _value = StringNotAllowedException.ThrowIfNotAllowed(
            value, nameof(TableName), AllowedCharacters);
    }

    public override string ToString() => Value;
}
