using PeachtreeBus.Data;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions;

/// <summary>
/// Defines an interface for publishing a message to all registered subscribers
/// </summary>
public interface ISubscribedPublisher
{
    Task<long> Publish(
        Topic topic,
        Type type,
        object message,
        UtcDateTime? notBefore = null,
        int priority = 0,
        UserHeaders? userHeaders = null);
}

/// <summary>
/// Publishes a subscription message to all current subscribers.
/// </summary>
public class SubscribedPublisher(
    IPublishPipelineInvoker pipelineInvoker)
    : ISubscribedPublisher
{
    private readonly IPublishPipelineInvoker _pipelineInvoker = pipelineInvoker;

    /// <summary>
    /// Publishes the message
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="type"></param>
    /// <param name="message"></param>
    /// <param name="notBefore"></param>
    /// <param name="priority"></param>
    /// <param name="userHeaders"></param>
    /// <returns>The number of subscribers that the message was published to.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public async Task<long> Publish(
        Topic topic,
        Type type,
        object message,
        UtcDateTime? notBefore = null,
        int priority = 0,
        UserHeaders? userHeaders = null)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        TypeIsNotISubscribedMessageException.ThrowIfMissingInterface(type);

        var context = new PublishContext
        {
            Message = message,
            Topic = topic,
            Headers = userHeaders ?? [],
            NotBefore = notBefore,
            Priority = priority,
        };

        // give the publish pipeline steps a chance to
        // alter the outgoing message.
        await _pipelineInvoker.Invoke(context);

        return context.RecipientCount ?? 0;
    }
}

public static class SubscriptionPublisherExtensions
{
    /// <summary>
    /// Publishes the message
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="publisher"></param>
    /// <param name="topic"></param>
    /// <param name="message"></param>
    /// <param name="notBefore"></param>
    /// <returns></returns>
    public static Task<long> PublishMessage<T>(
        this ISubscribedPublisher publisher,
        Topic topic,
        T message,
        DateTime? notBefore = null,
        int priority = 0,
        UserHeaders? userHeaders = null)
        where T : notnull
    {
        return publisher.Publish(topic, typeof(T), message, notBefore, priority, userHeaders);
    }
}
