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
        public Headers DeserializeHeaders(string headers) => JsonSerializer.Deserialize<Headers>(headers);
        public object DeserializeMessage(string message, Type type) => JsonSerializer.Deserialize(message, type);
        public object DeserializeSaga(string data, Type type) => JsonSerializer.Deserialize(data, type);
        public string SerializeHeaders(Headers headers) => JsonSerializer.Serialize(headers);
        public string SerializeMessage(object message, Type type) => JsonSerializer.Serialize(message, type);
        public string SerializeSaga(object data, Type type) => JsonSerializer.Serialize(data, type);
    }
}
