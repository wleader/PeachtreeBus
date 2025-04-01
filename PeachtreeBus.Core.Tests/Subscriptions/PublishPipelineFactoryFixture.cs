using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Tests.Pipeline;

namespace PeachtreeBus.Tests.Subscriptions
{
    [TestClass]
    public class PublishPipelineFactoryFixture
        : PipelineFactoryFixture<
            PublishContext,
            IPublishContext,
            IPublishPipeline,
            IFindPublishPipelineSteps,
            IPublishPipelineFinalStep>
    {
        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            _factory = new PublishPipelineFactory(_scope.Object);
        }

        protected override PublishContext CreateContext()
        {
            return TestData.CreatePublishContext();
        }
    }
}
