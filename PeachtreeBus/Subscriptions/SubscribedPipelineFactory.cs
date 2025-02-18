using PeachtreeBus.Pipelines;

namespace PeachtreeBus.Subscriptions
{
    /// <summary>
    /// Builds a pipeline for handling a Subscribed message
    /// </summary>
    public interface ISubscribedPipelineFactory : IPipelineFactory<ISubscribedContext, ISubscribedPipeline> { }

    /// <summary>
    /// Builds a pipeline for handling a Subscribed message
    /// </summary>
    public class SubscribedPipelineFactory(
        IWrappedScope scope)
        : PipelineFactory<ISubscribedContext, ISubscribedPipeline, IFindSubscribedPipelineSteps, ISubscribedHandlersPipelineStep>(scope)
        , ISubscribedPipelineFactory
    { }
}
