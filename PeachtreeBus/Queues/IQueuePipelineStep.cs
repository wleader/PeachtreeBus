using PeachtreeBus.Pipelines;

namespace PeachtreeBus.Queues
{
    public interface IQueuePipelineStep : IPipelineStep<QueueContext>
    {
        public int Priority { get; }
    }
}
