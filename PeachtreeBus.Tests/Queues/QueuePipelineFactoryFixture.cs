using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Queues;
using PeachtreeBus.Tests.Pipeline;

namespace PeachtreeBus.Tests.Queues
{
    [TestClass]
    public class QueuePipelineFactoryFixture
        : PipelineFactoryFixture<
            QueueContext,
            IQueueContext,
            IQueuePipeline,
            IFindQueuePipelineSteps,
            IQueuePipelineFinalStep>
    {
        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            _factory = new QueuePipelineFactory(_scope.Object);
        }

        protected override QueueContext CreateContext()
        {
            return TestData.CreateQueueContext();
        }
    }
}
