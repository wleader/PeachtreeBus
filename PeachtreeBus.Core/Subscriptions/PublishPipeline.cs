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
    IServiceProviderAccessor accessor)
    : PipelineFactory<PublishContext, IPublishContext, IPublishPipeline, IPublishPipelineStep, IPublishPipelineFinalStep>(accessor)
    , IPublishPipelineFactory;

public interface IPublishPipelineInvoker : IPipelineInvoker<PublishContext>;

public class PublishPipelineInvoker(
    IServiceProviderAccessor accessor)
    : OutgoingPipelineInvoker<PublishContext, IPublishContext, IPublishPipeline, IPublishPipelineFactory>(accessor)
    , IPublishPipelineInvoker;


