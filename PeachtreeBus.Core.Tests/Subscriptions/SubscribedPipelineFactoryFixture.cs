using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Tests.Pipeline;

namespace PeachtreeBus.Tests.Subscriptions
{
    [TestClass]
    public class SubscribedPipelineFactoryFixture
        : PipelineFactoryFixture<
            SubscribedContext,
            ISubscribedContext,
            ISubscribedPipeline,
            IFindSubscribedPipelineSteps,
            ISubscribedPipelineFinalStep>
    {
        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            _factory = new SubscribedPipelineFactory(_scope.Object);
        }

        protected override SubscribedContext CreateContext()
        {
            return TestData.CreateSubscribedContext();
        }
    }
}
