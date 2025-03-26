using PeachtreeBus.Queues;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

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

public abstract class PipelineFinalStep<TInternalContext, TContext> : IPipelineStep<TContext>
     where TInternalContext : Context
{
    public TInternalContext InternalContext { get; set; } = default!;

    // This property isn't used as the final step is always last in the pipeline
    // but it is requred by the interface.
    [ExcludeFromCodeCoverage]
    public int Priority { get => 0; }

    public abstract Task Invoke(TContext context, Func<TContext, Task>? next);
}

