using System.Threading.Tasks;

namespace PeachtreeBus.Pipelines;

public interface IPipelineInvoker<TContext>
{
    Task Invoke(TContext context);
}
