using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Pipelines
{
    public interface IPipelineStep<TContext>
    {
        Task Invoke(TContext context, Func<TContext, Task> next);
        public int Priority { get; }
    }
}
