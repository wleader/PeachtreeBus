using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Queues;
using PeachtreeBus.Tests.Pipeline;

namespace PeachtreeBus.Tests.Queues
{
    [TestClass]
    public class SendPipelineFactoryFixture
        : PipelineFactoryFixture<
            SendContext,
            ISendContext,
            ISendPipeline,
            IFindSendPipelineSteps,
            ISendPipelineFinalStep>
    {
        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            _factory = new SendPipelineFactory(_scope.Object);
        }

        protected override SendContext CreateContext()
        {
            return TestData.CreateSendContext();
        }
    }
}
