using System;
using System.Text.Json;

namespace PeachtreeBus
{
    public interface ISerializer
    {
        string SerializeHeaders(Headers headers);
        string SerializeMessage(object message, Type type);
        string SerializeSaga(object data, Type type);
        Headers DeserializeHeaders(string headers);
        object DeserializeMessage(string message, Type type);
        object DeserializeSaga(string data, Type type);
    }

    public class DefaultSerializer : ISerializer
    {
        public Headers DeserializeHeaders(string headers) => JsonSerializer.Deserialize<Headers>(headers)
            ?? throw new ArgumentException("Could not deserialize headers");
        public object DeserializeMessage(string message, Type type) => JsonSerializer.Deserialize(message, type)
            ?? throw new ArgumentException("Could not deserialize message");
        public object DeserializeSaga(string data, Type type) => JsonSerializer.Deserialize(data, type)
            ?? throw new ArgumentException("Could not deserialize saga data");
        public string SerializeHeaders(Headers headers) => JsonSerializer.Serialize(headers);
        public string SerializeMessage(object message, Type type) => JsonSerializer.Serialize(message, type);
        public string SerializeSaga(object data, Type type) => JsonSerializer.Serialize(data, type);
    }
}
