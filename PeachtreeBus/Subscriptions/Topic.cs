using System.Text.Json.Serialization;
using PeachtreeBus.Exceptions;
using PeachtreeBus.Serialization;

namespace PeachtreeBus.Subscriptions;

[JsonConverter(typeof(TopicJsonConverter))]
public readonly record struct Topic
{
    public const int MaxLength = 128;
    private readonly string _value;

    public string Value => _value
        ?? throw new TopicException("Topic is not initialized.");

    public Topic(string value)
    {
        TopicException.ThrowIfInvalid(value);
        _value = value;
    }

    public override string ToString() => Value;

    public class TopicJsonConverter()
        : PeachtreeBusJsonConverter<Topic, string>(v => new(v!), v => v.Value);
}

public class TopicException(string message) : PeachtreeBusException(message)
{
    public static void ThrowIfInvalid(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new TopicException(
                "A Topic cannot be null or whitespace.");

        if (value.Length > Topic.MaxLength)
            throw new TopicException(
                $"A Topic has a max length of {Topic.MaxLength} characters.");
    }
}

