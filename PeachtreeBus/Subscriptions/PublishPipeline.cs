using PeachtreeBus.Data;
using PeachtreeBus.Pipelines;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions;

public interface IPublishContext : IBaseOutgoingContext<SubscribedMessage>
{
    public Topic Topic { get; }
    public long? RecipientCount { get; set; }
}

public class PublishContext : BaseOutgoingContext<SubscribedMessage>, IPublishContext
{
    public required Topic Topic { get; set; }
    public long? RecipientCount { get; set; } = null;
}

public interface IPublishPipeline : IPipeline<IPublishContext>;

public class PublishPipeline : Pipeline<IPublishContext>, IPublishPipeline;

public interface IPublishPipelineFactory : IPipelineFactory<IPublishContext, IPublishPipeline>;

public class PublishPipelineFactory(
    IWrappedScope scope)
    : PipelineFactory<IPublishContext, IPublishPipeline, IFindPublishPipelineSteps, IPublishPipelinePublishStep>(scope)
    , IPublishPipelineFactory;

public interface IPublishPipelineInvoker : IPipelineInvoker<IPublishContext>;

public class PublishPipelineInvoker(
    IWrappedScope scope)
    : OutgoingPipelineInvoker<IPublishContext, IPublishPipeline, IPublishPipelineFactory>(scope)
    , IPublishPipelineInvoker;

public interface IFindPublishPipelineSteps : IFindPipelineSteps<IPublishContext>;

public interface IPublishPipelineStep : IPipelineStep<IPublishContext>;

public interface IPublishPipelinePublishStep : IPipelineStep<IPublishContext>;

public class PublishPipelinePublishStep(
    ISystemClock clock,
    IBusConfiguration configuration,
    ISerializer serializer,
    IBusDataAccess dataAccess,
    IPerfCounters perfCounters)
    : IPublishPipelinePublishStep
{
    private readonly ISystemClock _clock = clock;
    private readonly IBusConfiguration _configuration = configuration;
    private readonly ISerializer _serializer = serializer;
    private readonly IBusDataAccess _dataAccess = dataAccess;
    private readonly IPerfCounters _perfCounters = perfCounters;

    // This property isn't used as the handlers step is always last in the pipeline
    // but it is requred by the interface.
    [ExcludeFromCodeCoverage]
    public int Priority { get => 0; }

    public async Task Invoke(IPublishContext context, Func<IPublishContext, Task> next)
    {
        var message = context.Message;
        ArgumentNullException.ThrowIfNull(message, nameof(message));
        var type = context.Type;
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        TypeIsNotISubscribedMessageException.ThrowIfMissingInterface(type);

        // note the type in the headers so it can be deserialized.
        var headers = new Headers(type, context.UserHeaders);

        // create the message entity, serializing the headers and body.
        var sm = new SubscribedMessage
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
        _perfCounters.PublishMessage(context.RecipientCount.Value);
    }
}