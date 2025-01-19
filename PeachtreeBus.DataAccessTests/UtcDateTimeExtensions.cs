using PeachtreeBus.Model;

namespace PeachtreeBus.DataAccessTests;

public static class UtcDateTimeExtensions
{
    public static UtcDateTime AddMinutes(this UtcDateTime utcDateTime, double value) => new(utcDateTime.Value.AddMinutes(value));
    public static UtcDateTime AddHours(this UtcDateTime utcDateTime, double value) => new(utcDateTime.Value.AddHours(value));
    public static UtcDateTime AddMilliseconds(this UtcDateTime utcDateTime, double value) => new(utcDateTime.Value.AddMilliseconds(value));
    public static UtcDateTime AddDays(this UtcDateTime utcDateTime, double value) => new(utcDateTime.Value.AddDays(value));
}
