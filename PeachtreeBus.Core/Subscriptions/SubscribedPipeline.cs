using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Pipelines;

namespace PeachtreeBus.Subscriptions
{
    public interface ISubscribedPipeline : IPipeline<ISubscribedContext>;
    public class SubscribedPipeline : Pipeline<ISubscribedContext>, ISubscribedPipeline;

    /// <summary>
    /// Builds a pipeline for handling a Subscribed message
    /// </summary>
    public interface ISubscribedPipelineFactory : IPipelineFactory<SubscribedContext, ISubscribedContext, ISubscribedPipeline>;

    /// <summary>
    /// Builds a pipeline for handling a Subscribed message
    /// </summary>
    public class SubscribedPipelineFactory(
        IServiceProviderAccessor scope)
        : PipelineFactory<SubscribedContext, ISubscribedContext, ISubscribedPipeline, ISubscribedPipelineStep, ISubscribedPipelineFinalStep>(scope)
        , ISubscribedPipelineFactory;

    public interface ISubscribedPipelineInvoker : IPipelineInvoker<SubscribedContext>;

    public class SubscribedPipelineInvoker(
        IScopeFactory scopeFactory,
        ISharedDatabase sharedDatabase)
        : IncomingPipelineInvoker<SubscribedContext, ISubscribedContext, ISubscribedPipeline, ISubscribedPipelineFactory>(scopeFactory, sharedDatabase)
        , ISubscribedPipelineInvoker;
}
