using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Pipelines;

namespace PeachtreeBus.Subscriptions
{
    public interface ISubscribedPipelineInvoker : IPipelineInvoker<SubscribedContext> { }

    public class SubscribedPipelineInvoker(
        IWrappedScopeFactory scopeFactory,
        ISharedDatabase sharedDatabase)
        : PipelineInvoker<SubscribedContext, ISubscribedPipeline, ISubscribedPipelineFactory>(scopeFactory, sharedDatabase)
        , ISubscribedPipelineInvoker
    { }
}
