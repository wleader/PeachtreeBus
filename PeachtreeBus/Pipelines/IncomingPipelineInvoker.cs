using PeachtreeBus.DatabaseSharing;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Pipelines;

public interface IIncomingPipelineInvoker<TContext> : IPipelineInvoker<TContext>;

public abstract class IncomingPipelineInvoker<TContext, TPipeline, TFactory>(
    IWrappedScopeFactory scopeFactory,
    ISharedDatabase sharedDatabase)
    : IIncomingPipelineInvoker<TContext>
    where TContext : IBaseContext
    where TPipeline : IPipeline<TContext>
    where TFactory : IPipelineFactory<TContext, TPipeline>
{
    private readonly IWrappedScopeFactory _scopeFactory = scopeFactory;
    private readonly ISharedDatabase _sharedDatabase = sharedDatabase;

    public async Task Invoke(TContext context)
    {
        // the pipeline, and all its steps,
        // and all the handlers/sagas,
        // and all the object that they use need to be created fresh for each message.
        // to make this happen, we will start a DI scope, then create those objects.

        // There is one exception to this, the ISharedDatabase needs to be registered as scoped,
        // but we want to re-use the one we already have, because the message handling 
        // needs to re-use the same DB Transaction.

        // to accomplish this, we need to create the scope,
        // then create a shared database provider,
        // put the existing shared database connection in the provider,
        // then create the rest of the objects that will be needed.

        // doing it in this order means all the other objects are new,
        // except for the shared database.

        // the last piece of the puzzle is that the DI container will try to 
        // dipose IDisposable things in the scope. We don't want it to dispose the
        // Shared DB, so we set the Shared DB to ignore the .Dispose call for the
        // duration of the scope.

        ArgumentNullException.ThrowIfNull(context, nameof(context));

        // prevent the scope disposing from disposing our shared DB object.
        _sharedDatabase.DenyDispose = true;

        // create a scope.
        var scope = _scopeFactory.Create();
        try
        {
            context.SetScope(scope);

            // pass the database connection to the scope.
            var sharedDbProvider = scope.GetInstance<IShareObjectsBetweenScopes>();
            System.Diagnostics.Debug.Assert(sharedDbProvider.SharedDatabase is null);
            sharedDbProvider.SharedDatabase = _sharedDatabase;

            // now when we create the pipeline factory, it will re-use the shared DB connection,
            // and any objects it uses to build the pipeline will also re-use it.
            var pipelineFactory = (TFactory)scope.GetInstance(typeof(TFactory));
            var pipeline = pipelineFactory.Build();

            // invoke the pipeline.
            await pipeline.Invoke(context);
        }
        finally
        {
            // always clean up the scope.
            scope.Dispose();
            // always re-enable disposing of the shared database
            _sharedDatabase.DenyDispose = false;
        }
    }
}
