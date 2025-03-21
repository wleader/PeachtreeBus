using PeachtreeBus.Exceptions;
using PeachtreeBus.Serialization;
using System.Text.Json.Serialization;

namespace PeachtreeBus.Errors;

[JsonConverter(typeof(FailureCountJsonConverter))]
public readonly record struct FailureCount
{
    private readonly int _value;
    public int Value
    {
        get => FailureCountException.ThrowIfInvalid(_value);
    }

    public FailureCount(int value)
    {
        _value = FailureCountException.ThrowIfInvalid(value);
    }

    public override string ToString() => Value.ToString();

    public class FailureCountJsonConverter()
        : PeachtreeBusJsonConverter<FailureCount, int>(v => new(v), v => v.Value);

    public static implicit operator FailureCount(byte value) => new(value);
    public static implicit operator int(FailureCount failureCount) => failureCount.Value;
}

public class FailureCountException(string message) : PeachtreeBusException(message)
{
    public static int ThrowIfInvalid(int value)
    {
        if (value < 1)
            throw new FailureCountException("A Failure count can not be less than 1");
        return value;
    }
}
