using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Pipeline;
using PeachtreeBus.Queues;

namespace PeachtreeBus.Core.Tests.Queues
{
    [TestClass]
    public class SendPipelineFactoryFixture
        : PipelineFactoryFixture<
            SendContext,
            ISendContext,
            ISendPipeline,
            ISendPipelineStep,
            ISendPipelineFinalStep>
    {
        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            _factory = new SendPipelineFactory(_accessor.Object);
        }

        protected override SendContext CreateContext()
        {
            return TestData.CreateSendContext();
        }
    }
}
