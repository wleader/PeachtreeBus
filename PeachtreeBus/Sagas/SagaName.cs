using PeachtreeBus.Data;
using System.Text.Json.Serialization;

namespace PeachtreeBus.Sagas;

[JsonConverter(typeof(SagaNameJsonConverter))]
public readonly record struct SagaName
{
    public string Value { get; }

    public SagaName(string value)
    {
        DbSafeNameException.ThrowIfNotSafe(value, nameof(SagaName));
        Value = value;
    }

    public override string ToString() => Value
        ?? throw new DbSafeNameException($"{nameof(SagaName)} is not initialized.");

    public class SagaNameJsonConverter()
        : PeachtreeBusJsonConverter<SagaName, string>(v => new(v!), v => v.Value);

}
