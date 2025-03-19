using System.Linq;

namespace PeachtreeBus.Pipelines
{
    public interface IPipelineFactory<TInternalContext, TContext, TPipeline>
        where TPipeline : IPipeline<TContext>
    {
        TPipeline Build(TInternalContext context);
    }

    public abstract class PipelineFactory<TInternalContext, TContext, TPipeline, TFindPipelineSteps, TFinalStep>(
        IWrappedScope scope)
        : IPipelineFactory<TInternalContext, TContext, TPipeline>
        where TInternalContext : Context
        where TPipeline : IPipeline<TContext>
        where TFindPipelineSteps : IFindPipelineSteps<TContext>
        where TFinalStep : IPipelineFinalStep<TInternalContext, TContext>
    {
        private readonly IWrappedScope _scope = scope;

        public TPipeline Build(TInternalContext context)
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
            var handlersStep = (TFinalStep)_scope.GetInstance(typeof(TFinalStep));
            handlersStep.InternalContext = context;
            result.Add(handlersStep);

            // Pipeline is ready.
            return result;
        }
    }
}
