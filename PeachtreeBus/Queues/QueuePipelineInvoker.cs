using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Pipelines;

namespace PeachtreeBus.Queues
{
    public interface IQueuePipelineInvoker : IPipelineInvoker<IQueueContext> { }

    public class QueuePipelineInvoker(
        IWrappedScopeFactory scopeFactory,
        ISharedDatabase sharedDatabase)
        : PipelineInvoker<IQueueContext, IQueuePipeline, IQueuePipelineFactory>(scopeFactory, sharedDatabase)
        , IQueuePipelineInvoker
    { }
}
