using System.Text.Json.Serialization;

namespace PeachtreeBus.Sagas;

[JsonConverter(typeof(SagaKeyJsonConverter))]
public readonly record struct SagaKey
{
    public const int MaxLength = 128;
    public string Value { get; }
    public SagaKey(string value)
    {
        SagaKeyException.ThrowIfInvalid(value);
        Value = value;
    }

    public override string ToString() => Value
        ?? throw new SagaKeyException("SagaKey is not intialized");

    public class SagaKeyJsonConverter()
        : PeachtreeBusJsonConverter<SagaKey, string>(v => new(v!), v => v.Value);
}

public class SagaKeyException(string message) : PeachtreeBusException(message)
{
    public static void ThrowIfInvalid(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new SagaKeyException(
                "A SagaKey cannot be null or empty.");

        if (value.Length > SagaKey.MaxLength)
            throw new SagaKeyException(
                $"A SagaKey has a max length of {SagaKey.MaxLength} characters.");
    }
}
