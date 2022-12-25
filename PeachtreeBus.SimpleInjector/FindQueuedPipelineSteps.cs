using PeachtreeBus.Queues;
using System.Collections.Generic;
using System.Linq;

namespace PeachtreeBus.SimpleInjector
{
    /// <summary>
    /// An Implementation of IFindQueuePipelineSteps using SimpleInjector.
    /// </summary>
    public class FindQueuedPipelineSteps : IFindQueuePipelineSteps
    {
        private readonly IWrappedScope _scope;

        public FindQueuedPipelineSteps(IWrappedScope scope)
        {
            _scope = scope;
        }

        public IEnumerable<IQueuePipelineStep> FindSteps()
        {
            return _scope.GetAllInstances<IQueuePipelineStep>().ToList();
        }
    }
}
