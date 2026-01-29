using PeachtreeBus.Exceptions;
using PeachtreeBus.Serialization;
using System.Text.Json.Serialization;

namespace PeachtreeBus.Queues;

[JsonConverter(typeof(QueueNameJsonConverter))]
public readonly record struct QueueName
{
    public const string AllowedCharacters = "abcdefghijklmnopqrstuvwxyz0123456789_.";

    private readonly string _value;

    public string Value => _value
        ?? throw new NotInitializedException(typeof(QueueName));

    public QueueName(string value)
    {
        _value = StringNotAllowedException.ThrowIfNotAllowed(
            value, nameof(QueueName), AllowedCharacters);
    }

    public override string ToString() => Value;

    public class QueueNameJsonConverter()
        : PeachtreeBusJsonConverter<QueueName, string>(v => new(v!), v => v.Value);
}
