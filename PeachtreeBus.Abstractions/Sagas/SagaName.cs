using PeachtreeBus.Exceptions;
using PeachtreeBus.Serialization;
using System.Text.Json.Serialization;

namespace PeachtreeBus.Sagas;

[JsonConverter(typeof(SagaNameJsonConverter))]
public readonly record struct SagaName
{
    private readonly string _value;

    public string Value => _value
        ?? throw new NotInitializedException(typeof(SagaName));

    public SagaName(string value)
    {
        DbSafeNameException.ThrowIfNotSafe(value, nameof(SagaName));
        _value = value;
    }

    public override string ToString() => Value;

    public class SagaNameJsonConverter()
        : PeachtreeBusJsonConverter<SagaName, string>(v => new(v!), v => v.Value);
}
