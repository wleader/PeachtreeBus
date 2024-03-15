using PeachtreeBus.Pipelines;

namespace PeachtreeBus.Queues
{
    /// <summary>
    /// Builds a pipeline for handling a Queued message.
    /// </summary>
    public interface IQueuePipelineFactory : IPipelineFactory<QueueContext, IQueuePipeline> { }

    /// <summary>
    /// Builds a pipeline for handling a Queued message.
    /// </summary>
    public class QueuePipelineFactory(
        IWrappedScope scope)
        : PipelineFactory<QueueContext, IQueuePipeline, IFindQueuePipelineSteps, IQueueHandlersPipelineStep>(scope)
        , IQueuePipelineFactory
    { }

}
