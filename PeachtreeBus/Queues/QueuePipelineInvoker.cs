using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Pipelines;

namespace PeachtreeBus.Queues
{
    public interface IQueuePipelineInvoker : IPipelineInvoker<IQueueContext> { }

    public class QueuePipelineInvoker(
        IWrappedScopeFactory scopeFactory,
        ISharedDatabase sharedDatabase)
        : IncomingPipelineInvoker<IQueueContext, IQueuePipeline, IQueuePipelineFactory>(scopeFactory, sharedDatabase)
        , IQueuePipelineInvoker
    { }
}
