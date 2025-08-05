using PeachtreeBus.DatabaseSharing;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Pipelines;

public interface IIncomingPipelineInvoker<TContext> : IPipelineInvoker<TContext>;

public abstract class IncomingPipelineInvoker<TInternalContext, TContext, TPipeline, TFactory>(
    IScopeFactory scopeFactory,
    ISharedDatabase sharedDatabase)
    : IIncomingPipelineInvoker<TInternalContext>
    where TInternalContext : Context, TContext
    where TContext : IContext
    where TPipeline : IPipeline<TContext>
    where TFactory : IPipelineFactory<TInternalContext, TContext, TPipeline>
{
    public async Task Invoke(TInternalContext context)
    {
        // Invoke the pipeline, and all its steps,
        // and all the handlers/sagas,
        // and all the object that they use need to be created fresh for each message.
        // to make this happen, we will start a DI scope, then create those objects.

        // There is one exception to this, the ISharedDatabase needs to be registered as scoped,
        // but we want to re-use the one we already have, because the message handling 
        // needs to re-use the same DB Transaction.

        // to accomplish this, we need to create the scope,
        // then create an object to hold the shared database,
        // put the existing shared database connection into the sharing object,
        // then create the rest of the objects that will be needed.

        // doing it in this order means all the other objects are new,
        // except for the shared database. So any components that need a data
        // connection will use the existing one.

        // the last piece of the puzzle is that the DI container will try to 
        // dipose IDisposable things in the scope. We don't want it to dispose the
        // Shared DB, so we set the Shared DB to ignore the .Dispose call for the
        // duration of the scope.

        ArgumentNullException.ThrowIfNull(context, nameof(context));

        BusContextAccessor.Set(context);

        // prevent the scope for the message from disposing our DB object.
        sharedDatabase.DenyDispose = true;

        // create a scope.
        var accessor = scopeFactory.Create();
        try
        {
            context.ServiceProvider = accessor.ServiceProvider;

            // pass the database connection to the scope.
            var shareObjects = accessor.GetRequiredService<IShareObjectsBetweenScopes>();
            shareObjects.SharedDatabase = sharedDatabase;

            // when we create the pipeline factory, it will re-use the shared DB connection,
            // and any objects it uses to build the pipeline will also re-use it.
            var pipelineFactory = accessor.GetRequiredService<TFactory>();
            var pipeline = pipelineFactory.Build(context);

            // invoke the pipeline.
            await pipeline.Invoke(context);
        }
        finally
        {
            // always clean up the scope.
            accessor.Dispose();
            // always re-enable disposing of the shared database
            sharedDatabase.DenyDispose = false;
        }
    }
}
