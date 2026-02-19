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
        // For the outgoing pipeline:
        // It may be that the code is sending from 
        // an incoming message handler, in which case serviceProviderAccessor
        // will already have an existing DI scope stored inside it from when the
        // IncomingPipelineInvoker created a scope for handling the message.
        //
        // It could also be that there is a scope from some other framework.
        // and that other framework maybe does know to use IServiceProviderAccessor's
        // UseExisting() method to provide a scope to the PeachtreeBus library,
        // in which case we are fine to use that.
        //
        // Or it could be that some other framework created this OutgoingPipelineInvoker,
        // but did not call UseExisting to make it available to other scoped objects
        // like this one. In that case, reading serviceProviderAccessor.ServiceProvider
        // will throw an exception, and the user code will be forced to deal with it.

        ArgumentNullException.ThrowIfNull(context);

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
