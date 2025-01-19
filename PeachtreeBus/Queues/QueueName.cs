using PeachtreeBus.Data;

namespace PeachtreeBus.Queues;

public readonly record struct QueueName
{
    public string Value { get; }

    public QueueName(string value)
    {
        DbSafeNameException.ThrowIfNotSafe(value, nameof(QueueName));
        Value = value;
    }

    public override string ToString() => Value ?? throw new DbSafeNameException($"{nameof(QueueName)} is not initialized.");
}
