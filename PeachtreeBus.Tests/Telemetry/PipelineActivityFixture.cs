using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Abstractions.Tests.TestClasses;
using PeachtreeBus.Telemetry;
using System;
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
        var type = typeof(TestQueuePipelineStep);
        new PipelineActivity(type).Dispose();
        AssertActivity(_listener.Stopped.SingleOrDefault(), type);
    }

    public static void AssertActivity(Activity? activity, Type pipelineType) =>
        activity.AssertIsNotNull()
            .AssertOperationName("peachtreebus.pipeline " + pipelineType.Name)
            .AssertKind(ActivityKind.Internal)
            .AssertPipelineType(pipelineType)
            .AssertStarted();

    [TestMethod]
    public void Given_PipelineStepIsFinalStep_When_Activity_Then_NoActivity()
    {

        // We don't want 'Final' steps to be presented to the end user
        // as regular pipeline steps.
        new PipelineActivity(typeof(TestFinalStep)).Dispose();
        Assert.AreEqual(0, _listener.Stopped.Count);
    }
}
