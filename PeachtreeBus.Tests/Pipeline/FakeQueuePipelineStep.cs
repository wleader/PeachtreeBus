using PeachtreeBus.Pipelines;
using PeachtreeBus.Queues;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Pipeline
{
    public class FakeQueuePipelineStep(
        int priority = 0,
        Func<QueueContext, Func<QueueContext, Task>, Task>? handler = null)
        : FakePipelineStep<QueueContext>(priority, handler)
        , IQueuePipelineStep
    { }

    public class FakePipelineStep<TContext>(
        int priority = 0,
        Func<TContext, Func<TContext, Task>, Task>? handler = null)
        : IPipelineStep<TContext>
    {
        public int Priority { get; } = priority;

        readonly Func<TContext, Func<TContext, Task>, Task>? _handler = handler;

        public Task Invoke(TContext context, Func<TContext, Task> next)
        {
            return _handler?.Invoke(context, next) ?? Task.CompletedTask;
        }
    }
}
