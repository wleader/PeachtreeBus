using PeachtreeBus.Pipelines;
using PeachtreeBus.Queues;
using System.Collections.Generic;
using System.Linq;

namespace PeachtreeBus.SimpleInjector
{
    /// <summary>
    /// An Implementation of IFindQueuePipelineSteps using SimpleInjector.
    /// </summary>
    public class FindQueuedPipelineSteps(
        IWrappedScope scope)
        : IFindQueuePipelineSteps
    {
        private readonly IWrappedScope _scope = scope;

        public IEnumerable<IPipelineStep<QueueContext>> FindSteps()
        {
            return _scope.GetAllInstances<IQueuePipelineStep>().ToList();
        }
    }
}
