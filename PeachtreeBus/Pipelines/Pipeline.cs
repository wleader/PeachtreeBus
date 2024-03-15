using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Pipelines
{
    public class Pipeline<TContext> : IPipeline<TContext>
    {
        private PipelineLink<TContext>? _first;
        private PipelineLink<TContext>? _last;

        public void Add(IPipelineStep<TContext> step)
        {
            var link = new PipelineLink<TContext>(step);
            _first ??= link;
            _last?.SetNext(link);
            _last = link;
        }

        public Task Invoke(TContext context)
        {
            return _first?.Invoke(context)
                ?? throw new InvalidOperationException("Pipeline contains no steps.");
        }
    }
}
