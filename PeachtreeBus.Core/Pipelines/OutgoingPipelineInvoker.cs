using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Pipelines;

public interface IOutgoingPipelineInvoker<TContext> : IPipelineInvoker<TContext>;

public abstract class OutgoingPipelineInvoker<TInternalContext, TContext, TPipeline, TFactory>(
    IServiceProviderAccessor serviceProviderAccessor)
    : IOutgoingPipelineInvoker<TInternalContext>
    where TInternalContext : Context, TContext
    where TContext : IContext
    where TPipeline : IPipeline<TContext>
    where TFactory : class, IPipelineFactory<TInternalContext, TContext, TPipeline>
{
    public async Task Invoke(TInternalContext context)
    {
        // the outgoing pipeline is much simpler, since sending happens from an
        // existing DI scope. So we just need to use that existing scope to build
        // the pipeline and invoke it.

        ArgumentNullException.ThrowIfNull(context, nameof(context));

        BusContextAccessor.Set(context);

        context.ServiceProvider = serviceProviderAccessor.ServiceProvider;

        // when we create the pipeline factory, it will re-use the shared DB connection,
        // and any objects it uses to build the pipeline will also re-use it.
        var pipelineFactory = serviceProviderAccessor.GetRequiredService<TFactory>();
        var pipeline = pipelineFactory.Build(context);

        // invoke the pipeline.
        await pipeline.Invoke(context);
    }
}
