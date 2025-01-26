using System.Text.Json.Serialization;

namespace PeachtreeBus.Data;

[JsonConverter(typeof(IdentityJsonConverter))]
public readonly record struct Identity
{
    public long Value { get; }

    public Identity(long value) : this(value, true) { }

    private Identity(long value, bool validate)
    {
        if (validate)
            IdentityException.ThrowIfInvalid(value);
        Value = value;
    }

    public override string ToString() => Value.ToString();

    internal static readonly Identity Undefined = new(0, false);

    public class IdentityJsonConverter()
        : PeachtreeBusJsonConverter<Identity, long>(v => new(v), v => v.Value);
}

public class IdentityException(string message) : PeachtreeBusException(message)
{
    public static void ThrowIfInvalid(long value)
    {
        if (value < 1)
            throw new IdentityException($"Valid Identity range is 1 to {long.MaxValue}");
    }
}
