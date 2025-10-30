using PeachtreeBus.Exceptions;

namespace PeachtreeBus.DatabaseTestingShared;

public class DatabaseName(string value)
{
    private const string AllowedCharacters = "abcdefghijklmnopqrstuvwxyz0123456789_";

    private readonly string _value = StringNotAllowedException.ThrowIfNotAllowed(
        value, nameof(DatabaseName), AllowedCharacters);

    public string Value => _value
        ?? throw new NotInitializedException(typeof(DatabaseName));

    public override string ToString() => Value;

    public static implicit operator DatabaseName(string value) => new(value);
    public static implicit operator string(DatabaseName value) => value.Value;
}