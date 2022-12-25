using System.Threading.Tasks;

namespace PeachtreeBus.Pipelines
{
    public interface IPipeline<TContext>
    {
        void Add(IPipelineStep<TContext> steps);
        Task Invoke(TContext context);
    }
}
