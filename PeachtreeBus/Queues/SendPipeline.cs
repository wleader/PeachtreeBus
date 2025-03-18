using PeachtreeBus.Data;
using PeachtreeBus.Pipelines;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace PeachtreeBus.Queues;

public interface ISendContext : IBaseOutgoingContext<QueueMessage>
{
    public QueueName Destination { get; set; }
}

public class SendContext : BaseOutgoingContext<QueueMessage>, ISendContext
{
    public required QueueName Destination { get; set; }
}

public interface ISendPipeline : IPipeline<ISendContext>;

public class SendPipeline : Pipeline<ISendContext>, ISendPipeline;

public interface ISendPipelineFactory : IPipelineFactory<ISendContext, ISendPipeline>;

public class SendPipelineFactory(
    IWrappedScope scope)
    : PipelineFactory<ISendContext, ISendPipeline, IFindSendPipelineSteps, ISendPipelineSendStep>(scope)
    , ISendPipelineFactory;

public interface ISendPipelineInvoker : IPipelineInvoker<ISendContext>;

public class SendPipelineInvoker(
    IWrappedScope scope)
    : OutgoingPipelineInvoker<ISendContext, ISendPipeline, ISendPipelineFactory>(scope)
    , ISendPipelineInvoker;

public interface IFindSendPipelineSteps : IFindPipelineSteps<ISendContext>;

public interface ISendPipelineStep : IPipelineStep<ISendContext>;

public interface ISendPipelineSendStep : IPipelineStep<ISendContext>;

public class SendPipelineSendStep(
    ISystemClock clock,
    ISerializer serializer,
    IBusDataAccess dataAccess,
    IPerfCounters perfCounters)
    : ISendPipelineSendStep
{
    private readonly ISystemClock _clock = clock;
    private readonly ISerializer _serializer = serializer;
    private readonly IBusDataAccess _dataAccess = dataAccess;
    private readonly IPerfCounters _perfCounters = perfCounters;

    // This property isn't used as the handlers step is always last in the pipeline
    // but it is requred by the interface.
    [ExcludeFromCodeCoverage]
    public int Priority { get => 0; }

    public async Task Invoke(ISendContext context, Func<ISendContext, Task> next)
    {
        var message = context.Message;
        ArgumentNullException.ThrowIfNull(message, nameof(message));
        var type = context.Type;
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        TypeIsNotIQueueMessageException.ThrowIfMissingInterface(type);

        // note the type in the headers so it can be deserialized.
        var headers = new Headers(type, context.UserHeaders);

        // create the message entity, serializing the headers and body.
        var sm = new QueueMessage
        {
            MessageId = UniqueIdentity.New(), // will be ignored and the database will generate.
            Priority = context.Priority,
            NotBefore = context.NotBefore ?? _clock.UtcNow,
            Enqueued = _clock.UtcNow,
            Completed = null,
            Failed = null,
            Retries = 0,
            Headers = _serializer.SerializeHeaders(headers),
            Body = _serializer.SerializeMessage(message, type)
        };

        await _dataAccess.AddMessage(sm, context.Destination);
        _perfCounters.SentMessage();
    }
}
