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

public readonly record struct SerializedData
{
    public string Value { get; }

    public SerializedData(string value)
    {
        SerializedDataException.ThrowIfInvalid(value);
        Value = value;
    }

    public override string ToString() => Value ?? throw new SerializedDataException("SerializedData is not initialized.");
}
