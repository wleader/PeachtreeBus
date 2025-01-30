using System;
using System.Text.Json.Serialization;

namespace PeachtreeBus.Subscriptions;

[JsonConverter(typeof(SubscriberIdJsonConverter))]
public readonly record struct SubscriberId
{
    public Guid Value { get; }
    public SubscriberId(Guid value)
    {
        SubscriberIdException.ThrowIfInvalid(value);
        Value = value;
    }
    public override string ToString() => Value.ToString();

    public static SubscriberId New() => new(Guid.NewGuid());

    public class SubscriberIdJsonConverter()
        : PeachtreeBusJsonConverter<SubscriberId, Guid>(v => new(v), v => v.Value);
}

public class SubscriberIdException(string message) : PeachtreeBusException(message)
{
    public static void ThrowIfInvalid(Guid value)
    {
        if (value == Guid.Empty)
            throw new SubscriberIdException(
                $"A SubscriberId cannot be {Guid.Empty}.");
    }
}

public static class SubscriberIdExtensions
{
    public static SubscriberId RequreValid(this SubscriberId subscriberId)
    {
        SubscriberIdException.ThrowIfInvalid(subscriberId.Value);
        return subscriberId;
    }
}

