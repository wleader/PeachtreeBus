using System.Collections.Generic;

namespace PeachtreeBus.Queues
{
    public interface IFindQueuePipelineSteps
    {
        IEnumerable<IQueuePipelineStep> FindSteps();
    }
}
