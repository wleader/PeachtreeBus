using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Pipelines;

namespace PeachtreeBus.Queues;

public interface IQueuePipeline : IPipeline<IQueueContext>;

public class QueuePipeline : Pipeline<IQueueContext>, IQueuePipeline;

/// <summary>
/// Builds a pipeline for handling a Queued message.
/// </summary>
public interface IQueuePipelineFactory : IPipelineFactory<QueueContext, IQueueContext, IQueuePipeline>;

/// <summary>
/// Builds a pipeline for handling a Queued message.
/// </summary>
public class QueuePipelineFactory(
    IServiceProviderAccessor accessor)
    : PipelineFactory<QueueContext, IQueueContext, IQueuePipeline, IQueuePipelineStep, IQueuePipelineFinalStep>(accessor)
    , IQueuePipelineFactory;

public interface IQueuePipelineInvoker : IPipelineInvoker<QueueContext>;

public class QueuePipelineInvoker(
    IScopeFactory scopeFactory,
    ISharedDatabase sharedDatabase)
    : IncomingPipelineInvoker<QueueContext, IQueueContext, IQueuePipeline, IQueuePipelineFactory>(scopeFactory, sharedDatabase)
    , IQueuePipelineInvoker;
