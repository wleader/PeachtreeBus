using PeachtreeBus.Pipelines;

namespace PeachtreeBus.Queues
{
    public interface IQueuePipelineStep : IPipelineStep<IQueueContext> { }
}
