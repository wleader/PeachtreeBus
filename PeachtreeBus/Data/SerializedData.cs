using System.Text.Json.Serialization;
using PeachtreeBus.Serialization;

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
    private readonly string _value;

    public string Value => _value
         ?? throw new SerializedDataException("SerializedData is not initialized.");

    public SerializedData(string value)
    {
        SerializedDataException.ThrowIfInvalid(value);
        _value = value;
    }

    public override string ToString() => Value;

    public class SerializedDataJsonConverter()
        : PeachtreeBusJsonConverter<SerializedData, string>(s => new(s!), d => d.Value);
}
