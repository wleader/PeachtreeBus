using System.Linq;

namespace PeachtreeBus.Pipelines
{
    public interface IPipelineFactory<TInternalContext, TContext, TPipeline>
        where TPipeline : IPipeline<TContext>
    {
        TPipeline Build(TInternalContext context);
    }

    public abstract class PipelineFactory<TInternalContext, TContext, TPipeline, TPipelineStep, TFinalStep>(
        IServiceProviderAccessor serviceProviderAccessor)
        : IPipelineFactory<TInternalContext, TContext, TPipeline>
        where TInternalContext : Context
        where TPipeline : IPipeline<TContext>
        where TPipelineStep : IPipelineStep<TContext>
        where TFinalStep : IPipelineFinalStep<TContext>
    {
        public TPipeline Build(TInternalContext context)
        {
            // create a pipeline.
            var result = serviceProviderAccessor.GetRequiredService<TPipeline>();
            var steps = serviceProviderAccessor.GetServices<TPipelineStep>();

            // add the steps to the pipeline.
            foreach (var step in steps.OrderBy(s => s.Priority))
            {
                result.Add(step);
            }

            // the very last step in the chain is to pass the message to the
            // user handlers.
            var handlersStep = serviceProviderAccessor.GetRequiredService<TFinalStep>();
            result.Add(handlersStep);

            // Pipeline is ready.
            return result;
        }
    }
}
