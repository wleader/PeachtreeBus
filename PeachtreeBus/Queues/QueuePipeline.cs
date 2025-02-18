using PeachtreeBus.Pipelines;

namespace PeachtreeBus.Queues
{
    public interface IQueuePipeline : IPipeline<IQueueContext> { }

    public class QueuePipeline : Pipeline<IQueueContext>, IQueuePipeline { }
}
