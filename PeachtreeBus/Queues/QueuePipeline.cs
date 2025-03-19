using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Pipelines;

namespace PeachtreeBus.Queues
{
    public interface IQueuePipelineStep : IPipelineStep<IQueueContext>;

    public interface IFindQueuePipelineSteps : IFindPipelineSteps<IQueueContext> { }

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
        IWrappedScope scope)
        : PipelineFactory<QueueContext, IQueueContext, IQueuePipeline, IFindQueuePipelineSteps, IQueuePipelineFinalStep>(scope)
        , IQueuePipelineFactory;

    public interface IQueuePipelineInvoker : IPipelineInvoker<QueueContext>;

    public class QueuePipelineInvoker(
        IWrappedScopeFactory scopeFactory,
        ISharedDatabase sharedDatabase)
        : IncomingPipelineInvoker<QueueContext, IQueueContext, IQueuePipeline, IQueuePipelineFactory>(scopeFactory, sharedDatabase)
        , IQueuePipelineInvoker;
}
