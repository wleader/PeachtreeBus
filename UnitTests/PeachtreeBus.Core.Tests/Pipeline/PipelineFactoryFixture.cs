using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Pipelines;
using System.Collections.Generic;

namespace PeachtreeBus.Core.Tests.Pipeline;

[TestClass]
public abstract class PipelineFactoryFixture
    <TInternalContext, TContext, TPipeline, TPipelineStep, TFinalStep>
    where TInternalContext : Context
    where TContext : IContext
    where TPipeline : class, IPipeline<TContext>
    where TPipelineStep : class, IPipelineStep<TContext>
    where TFinalStep : class, IPipelineFinalStep<TContext>
{
    protected PipelineFactory<TInternalContext, TContext, TPipeline, TPipelineStep, TFinalStep> _factory = default!;
    protected Mock<TPipeline> _pipeline = default!;
    protected FakeServiceProviderAccessor _accessor = default!;
    protected Mock<TFinalStep> _finalStep = default!;

    protected TInternalContext _context = default!;

    public virtual void Initialize()
    {
        _pipeline = new();
        _accessor = new();
        _finalStep = new();

        _accessor.ServiceProviderMock.Setup(x => x.GetService(typeof(TPipeline))).Returns(() => _pipeline.Object);
        _accessor.ServiceProviderMock.Setup(x => x.GetService(typeof(TFinalStep))).Returns(() => _finalStep.Object);

        _context = CreateContext();
    }

    protected void Given_PipelineSteps(IEnumerable<TPipelineStep> steps)
    {
        _accessor.SetupService(() => steps);
    }

    protected abstract TInternalContext CreateContext();

    [TestMethod]
    public void Given_NoPipelineSteps_When_Build_Then_OnlyFinalStepIsAdded()
    {
        Given_PipelineSteps([]);

        var result = _factory.Build(_context);

        Assert.AreSame(_pipeline.Object, result);
        Assert.AreEqual(1, _pipeline.Invocations.Count);
        _pipeline.Verify(x => x.Add(_finalStep.Object), Times.Once);
    }

    [TestMethod]
    public void Given_PipelineSteps_When_Build_Then_StepsAreAdded()
    {
        var step1 = new Mock<TPipelineStep>();
        var step2 = new Mock<TPipelineStep>();
        var step3 = new Mock<TPipelineStep>();

        // note that priorities are out of order.
        step1.SetupGet(s => s.Priority).Returns(3);
        step2.SetupGet(s => s.Priority).Returns(2);
        step3.SetupGet(s => s.Priority).Returns(1);

        List<TPipelineStep> steps = [step1.Object, step2.Object, step3.Object];
        _accessor.SetupService<IEnumerable<TPipelineStep>>(() => steps);

        var result = _factory.Build(null!);

        Assert.IsTrue(ReferenceEquals(result, _pipeline.Object));

        // expect 3 steps plus one handler step.
        Assert.AreEqual(4, _pipeline.Invocations.Count);

        // check that the chain got built in priority order
        Assert.IsTrue(ReferenceEquals(step3.Object, _pipeline.Invocations[0].Arguments[0]));
        Assert.IsTrue(ReferenceEquals(step2.Object, _pipeline.Invocations[1].Arguments[0]));
        Assert.IsTrue(ReferenceEquals(step1.Object, _pipeline.Invocations[2].Arguments[0]));

        // handler must be last in the chain.
        Assert.IsTrue(ReferenceEquals(_finalStep.Object, _pipeline.Invocations[_pipeline.Invocations.Count - 1].Arguments[0]));
    }
}
