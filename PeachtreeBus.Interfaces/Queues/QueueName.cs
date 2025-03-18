using PeachtreeBus.Exceptions;
using PeachtreeBus.Serialization;
using System.Text.Json.Serialization;

namespace PeachtreeBus.Queues;

[JsonConverter(typeof(QueueNameJsonConverter))]
public readonly record struct QueueName
{
    private readonly string _value;

    public string Value => _value
        ?? throw new NotInitializedException(typeof(QueueName));

    public QueueName(string value)
    {
        DbSafeNameException.ThrowIfNotSafe(value, nameof(QueueName));
        _value = value;
    }

    public override string ToString() => Value;

    public class QueueNameJsonConverter()
        : PeachtreeBusJsonConverter<QueueName, string>(v => new(v!), v => v.Value);
}
