using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Pipeline;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Core.Tests.Subscriptions
{
    [TestClass]
    public class PublishPipelineFactoryFixture
        : PipelineFactoryFixture<
            PublishContext,
            IPublishContext,
            IPublishPipeline,
            IPublishPipelineStep,
            IPublishPipelineFinalStep>
    {
        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            _factory = new PublishPipelineFactory(_accessor);
        }

        protected override PublishContext CreateContext()
        {
            return TestData.CreatePublishContext();
        }
    }
}
