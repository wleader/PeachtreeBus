namespace PeachtreeBus.Pipelines;

/// <summary>
/// Defines the bottom step of a pipeline.
/// Invoke IHandleQueueMessage, IHandleSubscribed, Write to Queue, Publish, etc.
/// </summary>
/// <typeparam name="TInternalContext"></typeparam>
/// <typeparam name="TContext"></typeparam>
public interface IPipelineFinalStep<TInternalContext, TContext> : IPipelineStep<TContext>
    where TInternalContext : Context
{
    TInternalContext InternalContext { get; set; }
}
