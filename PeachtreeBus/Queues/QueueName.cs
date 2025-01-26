using PeachtreeBus.Data;
using System.Text.Json.Serialization;

namespace PeachtreeBus.Queues;

[JsonConverter(typeof(QueueNameJsonConverter))]
public readonly record struct QueueName
{
    public string Value { get; }

    public QueueName(string value)
    {
        DbSafeNameException.ThrowIfNotSafe(value, nameof(QueueName));
        Value = value;
    }

    public override string ToString() => Value ?? throw new DbSafeNameException($"{nameof(QueueName)} is not initialized.");

    public class QueueNameJsonConverter()
        : PeachtreeBusJsonConverter<QueueName, string>(v => new(v!), v => v.Value);
}
