using System.Text.Json.Serialization;

namespace PeachtreeBus.Data;

public class SerializedDataException : PeachtreeBusException
{

    internal SerializedDataException(string message) : base(message) { }

    public static void ThrowIfInvalid(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new SerializedDataException("SerializedData cannot be null or empty.");
    }
}

[JsonConverter(typeof(SerializedDataJsonConverter))]
public readonly record struct SerializedData
{
    public string Value { get; }

    public SerializedData(string value)
    {
        SerializedDataException.ThrowIfInvalid(value);
        Value = value;
    }

    public override string ToString() => Value ?? throw new SerializedDataException("SerializedData is not initialized.");

    public class SerializedDataJsonConverter()
        : PeachtreeBusJsonConverter<SerializedData, string>(s => new(s!), d => d.Value);
}
