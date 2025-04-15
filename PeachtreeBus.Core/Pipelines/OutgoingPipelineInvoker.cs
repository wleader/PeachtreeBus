using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Pipelines;

public interface IOutgoingPipelineInvoker<TContext> : IPipelineInvoker<TContext>;

public abstract class OutgoingPipelineInvoker<TInternalContext, TContext, TPipeline, TFactory>(
    IWrappedScope scope)
    : IOutgoingPipelineInvoker<TInternalContext>
    where TInternalContext : Context, TContext
    where TContext : IContext
    where TPipeline : IPipeline<TContext>
    where TFactory : class, IPipelineFactory<TInternalContext, TContext, TPipeline>
{
    private readonly IWrappedScope _scope = scope;

    public async Task Invoke(TInternalContext context)
    {
        // the outgoing pipeline is much simpler, since sending happens from an
        // existing DI scope. So we just need to use that existing scope to build
        // the pipeline and invoke it.

        ArgumentNullException.ThrowIfNull(context, nameof(context));

        context.Scope = _scope;

        // when we create the pipeline factory, it will re-use the shared DB connection,
        // and any objects it uses to build the pipeline will also re-use it.
        var pipelineFactory = _scope.GetInstance<TFactory>();
        var pipeline = pipelineFactory.Build(context);

        // invoke the pipeline.
        await pipeline.Invoke(context);
    }
}
