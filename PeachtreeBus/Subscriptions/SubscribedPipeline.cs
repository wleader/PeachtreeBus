using PeachtreeBus.Pipelines;

namespace PeachtreeBus.Subscriptions
{
    public interface ISubscribedPipeline : IPipeline<SubscribedContext> { }
    public class SubscribedPipeline : Pipeline<SubscribedContext>, ISubscribedPipeline { }
}
