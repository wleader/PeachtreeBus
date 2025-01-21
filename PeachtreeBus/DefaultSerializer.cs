using PeachtreeBus.Data;
using System;
using System.Text.Json;

namespace PeachtreeBus
{
    public interface ISerializer
    {
        SerializedData SerializeHeaders(Headers headers);
        SerializedData SerializeMessage(object message, Type type);
        SerializedData SerializeSaga(object data, Type type);
        Headers DeserializeHeaders(SerializedData headers);
        object DeserializeMessage(SerializedData message, Type type);
        object DeserializeSaga(SerializedData data, Type type);
    }

    public class DefaultSerializer : ISerializer
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = false,
        };

        public Headers DeserializeHeaders(SerializedData headers) => JsonSerializer.Deserialize<Headers>(headers.Value)
            ?? throw new ArgumentException("Could not deserialize headers");
        public object DeserializeMessage(SerializedData message, Type type) => JsonSerializer.Deserialize(message.Value, type)
            ?? throw new ArgumentException("Could not deserialize message");
        public object DeserializeSaga(SerializedData data, Type type) => JsonSerializer.Deserialize(data.Value, type)
            ?? throw new ArgumentException("Could not deserialize saga data");
        public SerializedData SerializeHeaders(Headers headers) => new(JsonSerializer.Serialize(headers, Options));
        public SerializedData SerializeMessage(object message, Type type) => new(JsonSerializer.Serialize(message, type, Options));
        public SerializedData SerializeSaga(object data, Type type) => new(JsonSerializer.Serialize(data, type, Options));
    }
}
