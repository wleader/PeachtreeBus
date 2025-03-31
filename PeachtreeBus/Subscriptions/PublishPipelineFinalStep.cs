using PeachtreeBus.Data;
using PeachtreeBus.Exceptions;
using PeachtreeBus.Pipelines;
using PeachtreeBus.Serialization;
using PeachtreeBus.Telemetry;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions;

public interface IPublishPipelineFinalStep : IPipelineFinalStep<PublishContext, IPublishContext>;

public class PublishPipelineFinalStep(
    ISystemClock clock,
    IBusConfiguration configuration,
    ISerializer serializer,
    IBusDataAccess dataAccess,
    IMeters meters)
    : PipelineFinalStep<PublishContext, IPublishContext>
    , IPublishPipelineFinalStep
{
    private readonly ISystemClock _clock = clock;
    private readonly IBusConfiguration _configuration = configuration;
    private readonly ISerializer _serializer = serializer;
    private readonly IBusDataAccess _dataAccess = dataAccess;
    private readonly IMeters _perfCounters = meters;

    public override async Task Invoke(IPublishContext context, Func<IPublishContext, Task>? next)
    {
        var message = context.Message;
        ArgumentNullException.ThrowIfNull(message, nameof(message));
        var type = message.GetType();
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        TypeIsNotISubscribedMessageException.ThrowIfMissingInterface(type);

        using var activity = new SendActivity(context);

        // note the type in the headers so it can be deserialized.
        var headers = new Headers(type, context.Headers);

        headers.Diagnostics = new(
            Activity.Current?.Id,
            context.StartNewConversation);

        // create the message entity, serializing the headers and body.
        var sm = new SubscribedData
        {
            ValidUntil = _clock.UtcNow.Add(_configuration.PublishConfiguration.Lifespan),
            MessageId = UniqueIdentity.Empty, // will be ignored and the database will generate.
            Priority = context.MessagePriority,
            NotBefore = context.NotBefore,
            Enqueued = _clock.UtcNow,
            Completed = null,
            Failed = null,
            Retries = 0,
            Headers = headers,
            Body = _serializer.Serialize(message, type),
            Topic = context.Topic,
        };

        context.RecipientCount = await _dataAccess.Publish(sm, context.Topic);
        _perfCounters.SentMessage(context.RecipientCount.Value);
    }
}