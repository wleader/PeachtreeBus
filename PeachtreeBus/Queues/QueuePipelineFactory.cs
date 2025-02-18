using PeachtreeBus.Pipelines;

namespace PeachtreeBus.Queues
{
    /// <summary>
    /// Builds a pipeline for handling a Queued message.
    /// </summary>
    public interface IQueuePipelineFactory : IPipelineFactory<IQueueContext, IQueuePipeline> { }

    /// <summary>
    /// Builds a pipeline for handling a Queued message.
    /// </summary>
    public class QueuePipelineFactory(
        IWrappedScope scope)
        : PipelineFactory<IQueueContext, IQueuePipeline, IFindQueuePipelineSteps, IQueueHandlersPipelineStep>(scope)
        , IQueuePipelineFactory
    { }

}
