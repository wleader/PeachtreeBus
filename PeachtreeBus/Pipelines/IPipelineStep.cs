using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Pipelines
{
    /// <summary>
    /// Defines the interface that users must implement to inject a pipeline step.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public interface IPipelineStep<TContext>
    {
        Task Invoke(TContext context, Func<TContext, Task> next);
        public int Priority { get; }
    }

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
}
