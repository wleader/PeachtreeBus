using PeachtreeBus.Data;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions;

/// <summary>
/// Publishes a subscription message to all current subscribers.
/// </summary>
public class SubscribedPublisher(
    ISystemClock clock,
    IPublishPipelineInvoker pipelineInvoker)
    : ISubscribedPublisher
{
    private readonly ISystemClock _clock = clock;
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
        ISubscribedMessage message,
        UtcDateTime? notBefore = null,
        int priority = 0,
        UserHeaders? userHeaders = null)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        var context = new PublishContext
        {
            Message = message,
            Topic = topic,
            Headers = userHeaders ?? [],
            NotBefore = notBefore ?? _clock.UtcNow,
            MessagePriority = priority,
        };

        // give the publish pipeline steps a chance to
        // alter the outgoing message.
        await _pipelineInvoker.Invoke(context);

        return context.RecipientCount ?? 0;
    }
}
