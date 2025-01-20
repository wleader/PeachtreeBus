using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Pipelines;
using PeachtreeBus.Queues;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Pipeline;

[TestClass]
public class PipelineLinkFixture
{
    [TestMethod]
    public async Task Given_NextIsNull_When_Invoke_Then_StepIsPassed_NullTask()
    {
        var step = new Mock<IPipelineStep<QueueContext>>();
        var link = new PipelineLink<QueueContext>(step.Object);
        link.SetNext(null!);
        var context = new QueueContext();

        await link.Invoke(context);

        step.Verify(s => s.Invoke(context, PipelineLink<QueueContext>.NullNext), Times.Once);
    }

    [TestMethod]
    public void When_NullNext_Then_Nothing()
    {
        var context = new QueueContext();
        var result = PipelineLink<QueueContext>.NullNext(context);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsCompleted);
    }
}
