using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Pipelines;

namespace PeachtreeBus.Queues
{
    public interface IQueuePipelineInvoker : IPipelineInvoker<QueueContext> { }

    public class QueuePipelineInvoker(
        IWrappedScopeFactory scopeFactory,
        ISharedDatabase sharedDatabase)
        : PipelineInvoker<QueueContext, IQueuePipeline, IQueuePipelineFactory>(scopeFactory, sharedDatabase)
        , IQueuePipelineInvoker
    { }
}
