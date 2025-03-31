using PeachtreeBus.Data;
using PeachtreeBus.Exceptions;
using PeachtreeBus.Sagas;
using System;
using System.Text.Json;

namespace PeachtreeBus.Serialization;

public interface ISerializer
{
    SerializedData Serialize<T>(T value);
    SerializedData Serialize(object value, Type type);
    T Deserialize<T>(SerializedData value);
    object Deserialize(SerializedData value, Type type);
}

public class DefaultSerializer : ISerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = false,
    };

    public SerializedData Serialize<T>(T value) => 
        new(JsonSerializer.Serialize(value, Options));

    public SerializedData Serialize(object value, Type type) =>
        new(JsonSerializer.Serialize(value, type,Options));

    public T Deserialize<T>(SerializedData value) =>
        JsonSerializer.Deserialize<T>(value.Value)
        ?? throw new SerializerException(value.Value, typeof(T));

    public object Deserialize(SerializedData value, Type type) =>
        JsonSerializer.Deserialize(value.Value, type)
        ?? throw new SerializerException(value.Value, type);
}
