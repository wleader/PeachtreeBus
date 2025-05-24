using PeachtreeBus.Pipelines;

namespace PeachtreeBus.Subscriptions;

public class PublishContext : OutgoingContext<SubscribedData>, IPublishContext
{
    public Topic Topic => Data.Topic;
    public long? RecipientCount { get; set; } = null;
}

public interface IPublishPipeline : IPipeline<IPublishContext>;

public class PublishPipeline : Pipeline<IPublishContext>, IPublishPipeline;

public interface IPublishPipelineFactory : IPipelineFactory<PublishContext, IPublishContext, IPublishPipeline>;

public class PublishPipelineFactory(
    IWrappedScope scope)
    : PipelineFactory<PublishContext, IPublishContext, IPublishPipeline, IFindPublishPipelineSteps, IPublishPipelineFinalStep>(scope)
    , IPublishPipelineFactory;

public interface IPublishPipelineInvoker : IPipelineInvoker<PublishContext>;

public class PublishPipelineInvoker(
    IWrappedScope scope)
    : OutgoingPipelineInvoker<PublishContext, IPublishContext, IPublishPipeline, IPublishPipelineFactory>(scope)
    , IPublishPipelineInvoker;


