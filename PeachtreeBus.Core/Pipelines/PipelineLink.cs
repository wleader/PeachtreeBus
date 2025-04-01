using PeachtreeBus.Telemetry;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Pipelines
{
    public class PipelineLink<TContext>(
        IPipelineStep<TContext> step)
    {
        private PipelineLink<TContext>? _next;
        private readonly IPipelineStep<TContext> _step = step;

        public void SetNext(PipelineLink<TContext> next)
        {
            _next = next;
        }

        public async Task Invoke(TContext context)
        {
            Func<TContext, Task> stepParam =
                _next is null
                ? NullNext
                : _next.Invoke;

            using var activity = new PipelineActivity(_step.GetType());

            await _step.Invoke(context, stepParam);
        }

        public static Task NullNext(TContext context)
        {
            return Task.CompletedTask;
        }
    }
}
