using System.Threading.Tasks;

namespace PeachtreeBus.Pipelines
{
    public class Pipeline<TContext> : IPipeline<TContext>
    {
        PipelineLink<TContext> _first;
        PipelineLink<TContext> _last;

        public void Add(IPipelineStep<TContext> step)
        {
            var link = new PipelineLink<TContext>(step);
            _first ??= link;
            _last?.SetNext(link);
            _last = link;
        }

        public Task Invoke(TContext context)
        {
            return _first?.Invoke(context);
        }
    }
}
