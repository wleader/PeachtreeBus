using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests;
using PeachtreeBus.Core.Tests.Pipeline;
using PeachtreeBus.Queues;

namespace PeachtreeBus.Core.Tests.Queues
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
