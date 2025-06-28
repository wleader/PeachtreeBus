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
    IServiceProviderAccessor accessor)
    : PipelineFactory<SendContext, ISendContext, ISendPipeline, ISendPipelineStep, ISendPipelineFinalStep>(accessor)
    , ISendPipelineFactory;

public interface ISendPipelineInvoker : IPipelineInvoker<SendContext>;

public class SendPipelineInvoker(
    IServiceProviderAccessor accessor)
    : OutgoingPipelineInvoker<SendContext, ISendContext, ISendPipeline, ISendPipelineFactory>(accessor)
    , ISendPipelineInvoker;

public interface ISendPipelineFinalStep : IPipelineFinalStep<ISendContext>;

public class SendPipelineFinalStep(
    ISystemClock clock,
    ISerializer serializer,
    IBusDataAccess dataAccess,
    IMeters meters)
    : PipelineFinalStep<ISendContext>
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

        var sendContext = (SendContext)context;

        var data = sendContext.Data;
        data.Headers.Diagnostics = new(
                Activity.Current?.Id,
                sendContext.StartNewConversation);
        data.Enqueued = _clock.UtcNow;
        data.Completed = null;
        data.Failed = null;
        data.Retries = 0;
        data.Body = _serializer.Serialize(message, type);
        data.MessageId = UniqueIdentity.New();

        await _dataAccess.AddMessage(data, context.Destination);
        _meters.SentMessage(1);
    }
}
