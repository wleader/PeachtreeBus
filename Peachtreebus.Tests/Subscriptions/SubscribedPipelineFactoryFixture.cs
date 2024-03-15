using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Subscriptions;
using PeachtreeBus;
using System.Collections.Generic;
using System;

namespace Peachtreebus.Tests.Subscriptions
{
    [TestClass]
    public class SubscribedPipelineFactoryFixture
    {
        private Mock<ISubscribedPipeline> _pipeline = default!;
        private Mock<IFindSubscribedPipelineSteps> _findSteps = default!;
        private Mock<IWrappedScope> _scope = default!;
        private Mock<ISubscribedHandlersPipelineStep> _handlersStep = default!;
        private SubscribedPipelineFactory _factory = default!;

        [TestInitialize]
        public void Init()
        {
            _pipeline = new();
            _findSteps = new();
            _scope = new();
            _handlersStep = new();

            _scope.Setup(s => s.GetInstance(typeof(ISubscribedPipeline))).Returns(_pipeline.Object);
            _scope.Setup(s => s.GetInstance(typeof(IFindSubscribedPipelineSteps))).Returns(_findSteps.Object);
            _scope.Setup(s => s.GetInstance(typeof(ISubscribedHandlersPipelineStep))).Returns(_handlersStep.Object);

            _factory = new SubscribedPipelineFactory(_scope.Object);
        }

        [TestMethod]
        public void Given_NoPipelineStepsAreProvided_When_Build_Then_OnlyHandlerStepIsAdded()
        {

            _findSteps.Setup(f => f.FindSteps()).Returns(Array.Empty<ISubscribedPipelineStep>());

            var result = _factory.Build();

            Assert.IsTrue(ReferenceEquals(result, _pipeline.Object));

            Assert.AreEqual(1, _pipeline.Invocations.Count);
            _pipeline.Verify(p => p.Add(_handlersStep.Object), Times.Once);
        }

        [TestMethod]
        public void Given_PipelineStepsAreProvided_When_Build_Then_StepsAreAdded()
        {
            var step1 = new Mock<ISubscribedPipelineStep>();
            var step2 = new Mock<ISubscribedPipelineStep>();
            var step3 = new Mock<ISubscribedPipelineStep>();

            // note that priorities are out of order.
            step1.SetupGet(s => s.Priority).Returns(3);
            step2.SetupGet(s => s.Priority).Returns(2);
            step3.SetupGet(s => s.Priority).Returns(1);

            List<ISubscribedPipelineStep> steps = [step1.Object, step2.Object, step3.Object];
            _findSteps.Setup(f => f.FindSteps()).Returns(steps);

            var result = _factory.Build();

            Assert.IsTrue(ReferenceEquals(result, _pipeline.Object));

            // expect 3 steps plus one handler step.
            Assert.AreEqual(4, _pipeline.Invocations.Count);

            // check that the chain got built in priority order
            Assert.IsTrue(ReferenceEquals(step3.Object, _pipeline.Invocations[0].Arguments[0]));
            Assert.IsTrue(ReferenceEquals(step2.Object, _pipeline.Invocations[1].Arguments[0]));
            Assert.IsTrue(ReferenceEquals(step1.Object, _pipeline.Invocations[2].Arguments[0]));

            // handler must be last in the chain.
            Assert.IsTrue(ReferenceEquals(_handlersStep.Object, _pipeline.Invocations[_pipeline.Invocations.Count - 1].Arguments[0]));
        }
    }
}
