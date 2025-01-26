using System.Text.Json.Serialization;

namespace PeachtreeBus.Data;

[JsonConverter(typeof(SchemaNameJsonConverter))]
public readonly record struct SchemaName
{
    public string Value { get; }

    public SchemaName(string value)
    {
        DbSafeNameException.ThrowIfNotSafe(value, nameof(SchemaName));
        Value = value;
    }

    public override string ToString() => Value
         ?? throw new DbSafeNameException($"{nameof(SchemaName)} is not initialized.");

    public class SchemaNameJsonConverter()
        : PeachtreeBusJsonConverter<SchemaName, string>(s => new(s!), s => s.Value);
}
