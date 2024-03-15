using PeachtreeBus.Pipelines;
using System.Linq;

namespace PeachtreeBus.Queues
{
    /// <summary>
    /// Builds a pipeline for handling a Queued message.
    /// </summary>
    public interface IQueuePipelineFactory
    {
        IQueuePipeline Build();
    }

    /// <summary>
    /// Builds a pipeline for handling a Queued message.
    /// </summary>
    public class QueuePipelineFactory(
        IWrappedScope scope)
        : IQueuePipelineFactory
    {
        private readonly IWrappedScope _scope = scope;

        public IQueuePipeline Build()
        {
            // create a pipeline.
            var result = _scope.GetInstance<IQueuePipeline>();

            // get the steps that should be in the pipeline.
            var findSteps = _scope.GetInstance<IFindQueuePipelineSteps>();
            // order the pipeline steps by priority.
            var steps = findSteps.FindSteps().OrderBy(s => s.Priority);

            // add the steps to the pipeline.
            foreach (var step in steps)
            {
                result.Add(step);
            }

            // the very last step in the chain is to pass the message to the
            // handlers.
            var handlersStep = _scope.GetInstance<IQueueHandlersPipelineStep>();
            result.Add(handlersStep);

            // Pipeline is ready.
            return result;
        }
    }
}
