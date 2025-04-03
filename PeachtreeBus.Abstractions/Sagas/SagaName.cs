using PeachtreeBus.Exceptions;
using PeachtreeBus.Serialization;
using System.Text.Json.Serialization;

namespace PeachtreeBus.Sagas;

[JsonConverter(typeof(SagaNameJsonConverter))]
public readonly record struct SagaName
{
    public const string AllowedCharacters = "abcdefghijklmnopqrstuvwxyz0123456789_";

    private readonly string _value;

    public string Value => _value
        ?? throw new NotInitializedException(typeof(SagaName));

    public SagaName(string value)
    {
        StringNotAllowedException.ThrowIfNotAllowed(
            value, nameof(SagaName), AllowedCharacters);
        _value = value;
    }

    public override string ToString() => Value;

    public class SagaNameJsonConverter()
        : PeachtreeBusJsonConverter<SagaName, string>(v => new(v!), v => v.Value);
}
