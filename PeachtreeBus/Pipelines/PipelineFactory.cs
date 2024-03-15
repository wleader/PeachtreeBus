using System.Linq;

namespace PeachtreeBus.Pipelines
{
    public interface IPipelineFactory<TContext, TPipeline>
        where TPipeline : IPipeline<TContext>
    {
        TPipeline Build();
    }

    public abstract class PipelineFactory<TContext, TPipeline, TFindPipelineSteps, THandlerStep>(
        IWrappedScope scope)
        : IPipelineFactory<TContext, TPipeline>
        where TPipeline : IPipeline<TContext>
        where TFindPipelineSteps : IFindPipelineSteps<TContext>
        where THandlerStep : IPipelineStep<TContext>
    {
        private readonly IWrappedScope _scope = scope;

        public TPipeline Build()
        {
            // create a pipeline.
            var result = (TPipeline)_scope.GetInstance(typeof(TPipeline));

            // get the steps that should be in the pipeline.
            var findSteps = (TFindPipelineSteps)_scope.GetInstance(typeof(TFindPipelineSteps));
            // order the pipeline steps by priority.
            var steps = findSteps.FindSteps().OrderBy(s => s.Priority);

            // add the steps to the pipeline.
            foreach (var step in steps)
            {
                result.Add(step);
            }

            // the very last step in the chain is to pass the message to the
            // handlers.
            var handlersStep = (THandlerStep)_scope.GetInstance(typeof(THandlerStep));
            result.Add(handlersStep);

            // Pipeline is ready.
            return result;
        }
    }
}
