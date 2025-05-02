using PeachtreeBus.ClassNames;
using PeachtreeBus.Data;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions;

/// <summary>
/// Publishes a subscription message to all current subscribers.
/// </summary>
public class SubscribedPublisher(
    ISystemClock clock,
    IPublishPipelineInvoker pipelineInvoker,
    IClassNameService classNameService)
    : ISubscribedPublisher
{
    private readonly ISystemClock _clock = clock;
    private readonly IPublishPipelineInvoker _pipelineInvoker = pipelineInvoker;
    private readonly IClassNameService _classNameService = classNameService;

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
        UserHeaders? userHeaders = null,
        bool newConversation = false)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        var data = new SubscribedData()
        {
            ValidUntil = default!,
            MessageId = UniqueIdentity.Empty, // will be ignored and the database will generate.
            Priority = priority,
            NotBefore = notBefore ?? _clock.UtcNow,
            Enqueued = _clock.UtcNow,
            Completed = null,
            Failed = null,
            Retries = 0,
            Headers = new()
            {
                MessageClass = _classNameService.GetClassNameForType(message.GetType()),
                UserHeaders = userHeaders ?? [],
            },
            Body = default!,
            Topic = topic,
        };

        var context = new PublishContext
        {
            Data = data,
            Message = message,
            StartNewConversation = newConversation
        };

        // give the publish pipeline steps a chance to
        // alter the outgoing message.
        await _pipelineInvoker.Invoke(context);

        return context.RecipientCount ?? 0;
    }
}
