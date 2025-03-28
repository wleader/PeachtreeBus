using PeachtreeBus.Data;
using PeachtreeBus.Exceptions;
using PeachtreeBus.Pipelines;
using PeachtreeBus.Serialization;
using PeachtreeBus.Telemetry;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PeachtreeBus.Queues;

public class SendContext : OutgoingContext<QueueData>, ISendContext
{
    public required QueueName Destination { get; set; }
}

public interface ISendPipeline : IPipeline<ISendContext>;

public class SendPipeline : Pipeline<ISendContext>, ISendPipeline;

public interface ISendPipelineFactory : IPipelineFactory<SendContext, ISendContext, ISendPipeline>;

public class SendPipelineFactory(
    IWrappedScope scope)
    : PipelineFactory<SendContext, ISendContext, ISendPipeline, IFindSendPipelineSteps, ISendPipelineFinalStep>(scope)
    , ISendPipelineFactory;

public interface ISendPipelineInvoker : IPipelineInvoker<SendContext>;

public class SendPipelineInvoker(
    IWrappedScope scope)
    : OutgoingPipelineInvoker<SendContext, ISendContext, ISendPipeline, ISendPipelineFactory>(scope)
    , ISendPipelineInvoker;

public interface ISendPipelineFinalStep : IPipelineFinalStep<SendContext, ISendContext>;

public class SendPipelineFinalStep(
    ISystemClock clock,
    ISerializer serializer,
    IBusDataAccess dataAccess,
    IMeters meters)
    : PipelineFinalStep<SendContext, ISendContext>
    , ISendPipelineFinalStep
{
    private readonly ISystemClock _clock = clock;
    private readonly ISerializer _serializer = serializer;
    private readonly IBusDataAccess _dataAccess = dataAccess;
    private readonly IMeters _meters = meters;

    public override async Task Invoke(ISendContext context, Func<ISendContext, Task>? next)
    {
        var message = context.Message;
        ArgumentNullException.ThrowIfNull(message, nameof(message));
        var type = message.GetType();
        TypeIsNotIQueueMessageException.ThrowIfMissingInterface(type);

        using var activity = new SendActivity(context);

        // note the type in the headers so it can be deserialized.
        var headers = new Headers(type, context.Headers);

        headers.Diagnostics.StartNewTraceOnReceive = InternalContext.StartNewConversation;
        headers.Diagnostics.TraceParent = Activity.Current?.Id;

        // create the message entity, serializing the headers and body.
        var sm = new QueueData
        {
            MessageId = UniqueIdentity.New(), // will be ignored and the database will generate.
            Priority = context.MessagePriority,
            NotBefore = context.NotBefore,
            Enqueued = _clock.UtcNow,
            Completed = null,
            Failed = null,
            Retries = 0,
            Headers = _serializer.SerializeHeaders(headers),
            Body = _serializer.SerializeMessage(message, type)
        };

        await _dataAccess.AddMessage(sm, context.Destination);
        _meters.SentMessage(1);
    }
}
