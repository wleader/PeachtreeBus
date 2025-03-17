using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Pipelines;

public interface IOutgoingPipelineInvoker<TContext> : IPipelineInvoker<TContext>;

public abstract class OutgoingPipelineInvoker<TContext, TPipeline, TFactory>(
    IWrappedScope scope)
    : IIncomingPipelineInvoker<TContext>
    where TContext : IBaseContext
    where TPipeline : IPipeline<TContext>
    where TFactory : IPipelineFactory<TContext, TPipeline>
{
    private readonly IWrappedScope _scope = scope;

    public async Task Invoke(TContext context)
    {
        // the outgoing pipeline is much simpler, since sending happens from an
        // existing DI scope. So we just need to use that existing scope to build
        // the pipeline and invoke it.

        ArgumentNullException.ThrowIfNull(context, nameof(context));

        context.Scope = _scope;
        // now when we create the pipeline factory, it will re-use the shared DB connection,
        // and any objects it uses to build the pipeline will also re-use it.
        var pipelineFactory = (TFactory)_scope.GetInstance(typeof(TFactory));
        var pipeline = pipelineFactory.Build();

        // invoke the pipeline.
        await pipeline.Invoke(context);
    }
}
