using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Pipeline;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Core.Tests.Subscriptions;

[TestClass]
public class PublishPipelineInvokerFixture : OutgoingPipelineInvokerFixtureBase<
    PublishPipelineInvoker, PublishContext, IPublishContext, IPublishPipeline, IPublishPipelineFactory, SubscribedData>
{
    protected override PublishContext CreateContext() => TestData.CreatePublishContext();

    protected override PublishPipelineInvoker CreateInvoker() => new(_accessor.Object);
}
