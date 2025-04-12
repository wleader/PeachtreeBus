using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests.Telemetry;
using PeachtreeBus.Pipelines;
using PeachtreeBus.Queues;
using PeachtreeBus.Telemetry;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Pipeline;

[TestClass]
public class PipelineLinkFixture
{
    private readonly QueueContext _context = TestData.CreateQueueContext();
    private readonly Mock<IPipelineStep<QueueContext>> _pipelineStep = new();

    private PipelineLink<QueueContext> _pipelineLink = default!;

    [TestInitialize]
    public void Initialize()
    {
        _pipelineStep.Reset();
        _pipelineLink = new(_pipelineStep.Object);
    }

    [TestMethod]
    public async Task Given_NextIsNull_When_Invoke_Then_StepIsPassed_NullTask()
    {
        _pipelineLink.SetNext(null!);

        await _pipelineLink.Invoke(_context);

        _pipelineStep.Verify(s => s.Invoke(_context, PipelineLink<QueueContext>.NullNext), Times.Once);
    }

    [TestMethod]
    public void When_NullNext_Then_Nothing()
    {
        var result = PipelineLink<QueueContext>.NullNext(_context);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsCompleted);
    }

    [TestMethod]
    public async Task When_Invoke_Then_ActivityIsStarted()
    {
        using var listener = new TestActivityListener(ActivitySources.User);

        await _pipelineLink.Invoke(_context);

        var activity = listener.ExpectOneCompleteActivity();
        PipelineActivityFixture.AssertActivity(activity, _pipelineStep.Object.GetType());
    }
}
