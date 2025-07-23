using PeachtreeBus.Serialization;
using System;
using System.Text.Json.Serialization;

namespace PeachtreeBus.Data;

/// <summary>
/// A DateTime class that is always in UTC.
/// Removes Ambiguity, and Allows for automatic conversion to and from the database.
/// </summary>
[JsonConverter(typeof(UtcDateTimeJsonConverter))]
public readonly record struct UtcDateTime
{
    private static readonly DateTime _uninitialized = new DateTime(0, DateTimeKind.Utc);

    private readonly DateTime _value;
    public DateTime Value => _value == DateTime.MinValue ? _uninitialized : _value;

    public UtcDateTime(DateTime value)
    {
        if (value.Kind == DateTimeKind.Unspecified)
            throw new ArgumentException($"The Kind property of value cannot be unspecified.", nameof(value));
        _value = value.ToUniversalTime();
    }

    public override string ToString() => Value.ToString();
    public string ToString(string? format) => Value.ToString(format);


    public static implicit operator DateTime(UtcDateTime utcDateTime) => utcDateTime.Value;
    public static implicit operator UtcDateTime(DateTime dateTime) => new(dateTime);

    internal class UtcDateTimeJsonConverter()
    : PeachtreeBusJsonConverter<UtcDateTime, DateTime>
    (u => new(u), u => u.Value);
}

public static class UtcDateTimeExtensions
{
    public static UtcDateTime AddMinutes(this UtcDateTime utcDateTime, double value) => new(utcDateTime.Value.AddMinutes(value));
    public static UtcDateTime AddHours(this UtcDateTime utcDateTime, double value) => new(utcDateTime.Value.AddHours(value));
    public static UtcDateTime AddMilliseconds(this UtcDateTime utcDateTime, double value) => new(utcDateTime.Value.AddMilliseconds(value));
    public static UtcDateTime AddDays(this UtcDateTime utcDateTime, double value) => new(utcDateTime.Value.AddDays(value));
}
