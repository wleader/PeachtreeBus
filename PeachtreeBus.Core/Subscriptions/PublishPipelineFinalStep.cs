using PeachtreeBus.ClassNames;
using PeachtreeBus.Data;
using PeachtreeBus.Exceptions;
using PeachtreeBus.Pipelines;
using PeachtreeBus.Serialization;
using PeachtreeBus.Telemetry;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions;

public interface IPublishPipelineFinalStep : IPipelineFinalStep<IPublishContext>;

public class PublishPipelineFinalStep(
    ISystemClock clock,
    IBusConfiguration configuration,
    ISerializer serializer,
    IBusDataAccess dataAccess,
    IMeters meters)
    : PipelineFinalStep<IPublishContext>
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

        var publishContext = (PublishContext)context;
        var data = publishContext.Data;
        data.Headers.Diagnostics = new(
            Activity.Current?.Id,
            context.StartNewConversation);
        data.MessageId = default; // will be database generated.
        data.ValidUntil = _clock.UtcNow.Add(_configuration.PublishConfiguration.Lifespan);
        data.Enqueued = _clock.UtcNow;
        data.Completed = null;
        data.Failed = null;
        data.Retries = 0;
        data.Body = _serializer.Serialize(message, type);

        context.RecipientCount = await _dataAccess.Publish(data, context.Topic);
        _perfCounters.SentMessage(context.RecipientCount.Value);
    }
}