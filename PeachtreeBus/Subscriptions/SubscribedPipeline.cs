using PeachtreeBus.Pipelines;

namespace PeachtreeBus.Subscriptions
{
    public interface ISubscribedPipeline : IPipeline<ISubscribedContext> { }
    public class SubscribedPipeline : Pipeline<ISubscribedContext>, ISubscribedPipeline { }
}
