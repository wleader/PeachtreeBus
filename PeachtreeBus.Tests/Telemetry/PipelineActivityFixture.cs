using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Abstractions.Tests.TestClasses;
using PeachtreeBus.Queues;
using PeachtreeBus.Telemetry;
using System.Diagnostics;
using System.Linq;

namespace PeachtreeBus.Tests.Telemetry;

[TestClass]
public class PipelineActivityFixture()
    : ActivityFixtureBase(ActivitySources.User)
{
    [TestMethod]
    public void When_Activity_Then_TagsAreCorrect()
    {
        var pipeline = new TestQueuePipelineStep();
        var type = pipeline.GetType();
        new PipelineActivity<IQueueContext>(pipeline).Dispose();

        _listener.Stopped.SingleOrDefault()
            .AssertIsNotNull()
            .AssertOperationName("peachtreebus.pipeline " + type.Name)
            .AssertKind(ActivityKind.Internal)
            .AssertPipelineType(type)
            .AssertStarted();
    }

    [TestMethod]
    public void Given_PipelineStepIsFinalStep_When_Activity_Then_NoActivity()
    {

        // We don't want 'Final' steps to be presented to the end user
        // as regular pipeline steps.
        new PipelineActivity<IQueueContext>(new TestFinalStep()).Dispose();
        Assert.AreEqual(0, _listener.Stopped.Count);
    }
}
