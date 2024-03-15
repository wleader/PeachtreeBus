using PeachtreeBus.Pipelines;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System;
using System.Threading.Tasks;

namespace Peachtreebus.Tests.Pipeline
{
    public class FakeQueuePipelineStep(
        int priority = 0,
        Func<QueueContext, Func<QueueContext, Task>, Task>? handler = null)
        : FakePipelineStep<QueueContext>(priority, handler)
        , IQueuePipelineStep
    { }

    public class FakeSubscribedPipelineStep(
        int priority = 0,
        Func<SubscribedContext, Func<SubscribedContext, Task>, Task>? handler = null)
        : FakePipelineStep<SubscribedContext>(priority, handler)
        , ISubscribedPipelineStep
    { }

    public class FakePipelineStep<TContext>(
        int priority = 0,
        Func<TContext, Func<TContext, Task>, Task>? handler = null)
        : IPipelineStep<TContext>
    {
        public int Priority { get; set; } = priority;

        readonly Func<TContext, Func<TContext, Task>, Task>? _handler = handler;

        public Task Invoke(TContext context, Func<TContext, Task> next)
        {
            return _handler?.Invoke(context, next) ?? Task.CompletedTask;
        }
    }
}
