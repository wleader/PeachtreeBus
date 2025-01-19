using System;

namespace PeachtreeBus.Model;

public readonly record struct UtcDateTime
{
    public DateTime Value { get; }

    public UtcDateTime(DateTime value)
    {
        if (value.Kind == DateTimeKind.Unspecified)
            throw new ArgumentException($"The Kind property of value cannot be unspecified.", nameof(value));

        Value = value.ToUniversalTime();
    }

    public static implicit operator DateTime(UtcDateTime utcDateTime) => utcDateTime.Value.ToUniversalTime();
    public static implicit operator UtcDateTime(DateTime dateTime) => new(dateTime);
}
