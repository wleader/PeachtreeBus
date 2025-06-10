using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Pipelines;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System.Collections.Generic;
using System.Linq;

namespace PeachtreeBus.Core.Tests.Pipeline;

public abstract class FindPipelineStepsFixture<TFinder, TContext, TPipelineStep>
    where TFinder : FindPipelineSteps<TContext, TPipelineStep>
    where TPipelineStep : class, IPipelineStep<TContext>
{
    private TFinder _testSubject = default!;
    protected Mock<IWrappedScope> _scope = new();
    protected abstract TFinder CreateFinder();

    protected List<TPipelineStep> _steps = [];

    [TestInitialize]
    public void Initialize()
    {
        _scope.Reset();
        _scope.Setup(x => x.GetAllInstances<TPipelineStep>()).Returns(() => _steps);
        _testSubject = CreateFinder();
    }

    [TestMethod]
    public void Given_ScopeHasNoSteps_When_FindSteps_Then_NoSteps()
    {
        _steps.Clear();
        CollectionAssert.AreEquivalent(_steps, _testSubject.FindSteps().ToList());
    }

    [TestMethod]
    public void Given_ScopeHasSteps_When_FindSteps_Then_NoSteps()
    {
        _steps.Clear();
        _steps.Add(new Mock<TPipelineStep>().Object);
        _steps.Add(new Mock<TPipelineStep>().Object);

        CollectionAssert.AreEquivalent(_steps, _testSubject.FindSteps().ToList());
    }
}

[TestClass]
public class FindQueuePipelineStepsFixture
    : FindPipelineStepsFixture<FindQueuedPipelineSteps, IQueueContext, IQueuePipelineStep>
{
    protected override FindQueuedPipelineSteps CreateFinder() => new(_scope.Object);
}


[TestClass]
public class FindSubscribedPipelineStepsFixture
    : FindPipelineStepsFixture<FindSubscribedPipelineSteps, ISubscribedContext, ISubscribedPipelineStep>
{
    protected override FindSubscribedPipelineSteps CreateFinder() => new(_scope.Object);
}


[TestClass]
public class FindSendPipelineStepsFixture
    : FindPipelineStepsFixture<FindSendPipelineSteps, ISendContext, ISendPipelineStep>
{
    protected override FindSendPipelineSteps CreateFinder() => new(_scope.Object);
}


[TestClass]
public class FindPublishPipelineStepsFixture
    : FindPipelineStepsFixture<FindPublishPipelineSteps, IPublishContext, IPublishPipelineStep>
{
    protected override FindPublishPipelineSteps CreateFinder() => new(_scope.Object);
}