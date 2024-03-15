using PeachtreeBus.Pipelines;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System;
using System.Threading.Tasks;

namespace Peachtreebus.Tests.Pipeline
{
    public class FakeQueuePipelineStep : FakePipelineStep<QueueContext>, IQueuePipelineStep
    {
        public FakeQueuePipelineStep(int priority = 0, Func<QueueContext, Func<QueueContext, Task>?, Task>? handler = null)
            : base(priority, handler) { }
    }

    public class FakeSubscribedPipelineStep : FakePipelineStep<SubscribedContext>, ISubscribedPipelineStep
    {
        public FakeSubscribedPipelineStep(int priority = 0, Func<SubscribedContext, Func<SubscribedContext, Task>?, Task>? handler = null)
            : base(priority, handler) { }
    }

    public class FakePipelineStep<TContext> : IPipelineStep<TContext>
    {
        public int Priority { get; set; }

        readonly Func<TContext, Func<TContext, Task>?, Task>? _handler = null;

        public FakePipelineStep(int priority = 0, Func<TContext, Func<TContext, Task>?, Task>? handler = null)
        {
            Priority = priority;
            _handler = handler;
        }

        public Task Invoke(TContext context, Func<TContext, Task>? next)
        {
            return _handler?.Invoke(context, next) ?? Task.CompletedTask;
        }
    }
}
