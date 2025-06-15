using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Pipelines;
using System.Collections.Generic;

namespace PeachtreeBus.Core.Tests.Pipeline;

[TestClass]
public abstract class PipelineFactoryFixture
    <TInternalContext, TContext, TPipeline, TFindSteps, TFinalStep>
    where TInternalContext : Context
    where TContext : IContext
    where TPipeline : class, IPipeline<TContext>
    where TFindSteps : class, IFindPipelineSteps<TContext>
    where TFinalStep : class, IPipelineFinalStep<TContext>
{
    protected PipelineFactory<TInternalContext, TContext, TPipeline, TFindSteps, TFinalStep> _factory = default!;
    protected Mock<TPipeline> _pipeline = default!;
    protected Mock<IWrappedScope> _scope = default!;
    protected Mock<TFindSteps> _findSteps = default!;
    protected Mock<TFinalStep> _finalStep = default!;

    protected TInternalContext _context = default!;

    public virtual void Initialize()
    {
        _pipeline = new();
        _scope = new();
        _findSteps = new();
        _finalStep = new();

        _scope.Setup(x => x.GetInstance(typeof(TPipeline))).Returns(() => _pipeline.Object);
        _scope.Setup(x => x.GetInstance(typeof(TFindSteps))).Returns(() => _findSteps.Object);
        _scope.Setup(x => x.GetInstance(typeof(TFinalStep))).Returns(() => _finalStep.Object);

        _context = CreateContext();
    }

    protected void Given_PipelineSteps(IEnumerable<IPipelineStep<TContext>> steps)
    {
        _findSteps.Setup(x => x.FindSteps()).Returns(steps);
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
        var step1 = new Mock<IPipelineStep<TContext>>();
        var step2 = new Mock<IPipelineStep<TContext>>();
        var step3 = new Mock<IPipelineStep<TContext>>();

        // note that priorities are out of order.
        step1.SetupGet(s => s.Priority).Returns(3);
        step2.SetupGet(s => s.Priority).Returns(2);
        step3.SetupGet(s => s.Priority).Returns(1);

        List<IPipelineStep<TContext>> steps = [step1.Object, step2.Object, step3.Object];
        _findSteps.Setup(f => f.FindSteps()).Returns(steps);

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
