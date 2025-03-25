using PeachtreeBus.Data;
using PeachtreeBus.Exceptions;
using PeachtreeBus.Pipelines;
using PeachtreeBus.Serialization;
using PeachtreeBus.Telemetry;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions;

public interface IPublishPipelineFinalStep : IPipelineFinalStep<PublishContext, IPublishContext>;

public class PublishPipelineFinalStep(
    ISystemClock clock,
    IBusConfiguration configuration,
    ISerializer serializer,
    IBusDataAccess dataAccess,
    IMeters meters)
    : IPublishPipelineFinalStep
{
    private readonly ISystemClock _clock = clock;
    private readonly IBusConfiguration _configuration = configuration;
    private readonly ISerializer _serializer = serializer;
    private readonly IBusDataAccess _dataAccess = dataAccess;
    private readonly IMeters _perfCounters = meters;

    // This property isn't used as the handlers step is always last in the pipeline
    // but it is requred by the interface.
    [ExcludeFromCodeCoverage]
    public int Priority { get => 0; }

    public PublishContext InternalContext { get; set; } = default!;

    public async Task Invoke(IPublishContext context, Func<IPublishContext, Task> next)
    {
        var message = context.Message;
        ArgumentNullException.ThrowIfNull(message, nameof(message));
        var type = message.GetType();
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        TypeIsNotISubscribedMessageException.ThrowIfMissingInterface(type);

        // note the type in the headers so it can be deserialized.
        var headers = new Headers(type, context.Headers);

        // create the message entity, serializing the headers and body.
        var sm = new SubscribedData
        {
            ValidUntil = _clock.UtcNow.Add(_configuration.PublishConfiguration.Lifespan),
            MessageId = UniqueIdentity.Empty, // will be ignored and the database will generate.
            Priority = context.Priority,
            NotBefore = context.NotBefore ?? _clock.UtcNow,
            Enqueued = _clock.UtcNow,
            Completed = null,
            Failed = null,
            Retries = 0,
            Headers = _serializer.SerializeHeaders(headers),
            Body = _serializer.SerializeMessage(message, type)
        };

        context.RecipientCount = await _dataAccess.Publish(sm, context.Topic);
        _perfCounters.SentMessage(context.RecipientCount.Value);
    }
}