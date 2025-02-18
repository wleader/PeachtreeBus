using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Pipelines;

namespace PeachtreeBus.Subscriptions
{
    public interface ISubscribedPipelineInvoker : IPipelineInvoker<ISubscribedContext> { }

    public class SubscribedPipelineInvoker(
        IWrappedScopeFactory scopeFactory,
        ISharedDatabase sharedDatabase)
        : PipelineInvoker<ISubscribedContext, ISubscribedPipeline, ISubscribedPipelineFactory>(scopeFactory, sharedDatabase)
        , ISubscribedPipelineInvoker
    { }
}
