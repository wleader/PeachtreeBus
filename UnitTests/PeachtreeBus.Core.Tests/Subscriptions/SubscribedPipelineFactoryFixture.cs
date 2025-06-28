using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Pipeline;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Core.Tests.Subscriptions
{
    [TestClass]
    public class SubscribedPipelineFactoryFixture
        : PipelineFactoryFixture<
            SubscribedContext,
            ISubscribedContext,
            ISubscribedPipeline,
            ISubscribedPipelineStep,
            ISubscribedPipelineFinalStep>
    {
        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            _factory = new SubscribedPipelineFactory(_accessor);
        }

        protected override SubscribedContext CreateContext()
        {
            return TestData.CreateSubscribedContext();
        }
    }
}
