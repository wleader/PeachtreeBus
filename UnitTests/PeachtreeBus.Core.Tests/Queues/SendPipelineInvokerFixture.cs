using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Pipeline;
using PeachtreeBus.Queues;

namespace PeachtreeBus.Core.Tests.Queues;

[TestClass]
public class SendPipelineInvokerFixture : OutgoingPipelineInvokerFixtureBase<
    SendPipelineInvoker, SendContext, ISendContext, ISendPipeline, ISendPipelineFactory, QueueData>
{
    protected override SendPipelineInvoker CreateInvoker() => new(_accessor);

    protected override SendContext CreateContext() => TestData.CreateSendContext();
}
