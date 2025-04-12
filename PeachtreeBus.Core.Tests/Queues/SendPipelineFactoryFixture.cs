using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests;
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
