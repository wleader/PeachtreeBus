using PeachtreeBus.Pipelines;

namespace PeachtreeBus.Queues
{
    public interface IQueuePipeline : IPipeline<QueueContext> { }

    public class QueuePipeline : Pipeline<QueueContext>, IQueuePipeline { }
}
