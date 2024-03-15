using PeachtreeBus.Pipelines;
using System.Linq;

namespace PeachtreeBus.Subscriptions
{
    /// <summary>
    /// Builds a pipeline for handling a Subscribed message
    /// </summary>
    public interface ISubscribedPipelineFactory
    {
        ISubscribedPipeline Build();
    }

    /// <summary>
    /// Builds a pipeline for handling a Subscribed message
    /// </summary>
    public class SubscribedPipelineFactory(
        IWrappedScope scope)
        : PipelineFactory<SubscribedContext, ISubscribedPipeline, IFindSubscribedPipelineSteps, ISubscribedHandlersPipelineStep>(scope)
        , ISubscribedPipelineFactory
    { }
}
