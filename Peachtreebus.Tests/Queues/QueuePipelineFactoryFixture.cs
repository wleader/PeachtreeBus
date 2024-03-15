using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus;
using PeachtreeBus.Queues;
using System;
using System.Collections.Generic;

namespace Peachtreebus.Tests.Queues
{
    [TestClass]
    public class QueuePipelineFactoryFixture
    {
        private Mock<IQueuePipeline> _pipeline = default!;
        private Mock<IFindQueuePipelineSteps> _findSteps = default!;
        private Mock<IWrappedScope> _scope = default!;
        private Mock<IQueueHandlersPipelineStep> _handlersStep = default!;
        private QueuePipelineFactory _factory = default!;

        [TestInitialize]
        public void Init()
        {
            _pipeline = new();
            _findSteps = new();
            _scope = new();
            _handlersStep = new();

            _scope.Setup(s => s.GetInstance(typeof(IQueuePipeline))).Returns(_pipeline.Object);
            _scope.Setup(s => s.GetInstance(typeof(IFindQueuePipelineSteps))).Returns(_findSteps.Object);
            _scope.Setup(s => s.GetInstance(typeof(IQueueHandlersPipelineStep))).Returns(_handlersStep.Object);

            _factory = new QueuePipelineFactory(_scope.Object);
        }

        [TestMethod]
        public void Given_NoPipelineStepsAreProvided_When_Build_Then_OnlyHandlerStepIsAdded()
        {

            _findSteps.Setup(f => f.FindSteps()).Returns(Array.Empty<IQueuePipelineStep>());

            var result = _factory.Build();

            Assert.IsTrue(ReferenceEquals(result, _pipeline.Object));

            Assert.AreEqual(1, _pipeline.Invocations.Count);
            _pipeline.Verify(p => p.Add(_handlersStep.Object), Times.Once);
        }

        [TestMethod]
        public void Given_PipelineStepsAreProvided_When_Build_Then_StepsAreAdded()
        {
            var step1 = new Mock<IQueuePipelineStep>();
            var step2 = new Mock<IQueuePipelineStep>();
            var step3 = new Mock<IQueuePipelineStep>();

            // note that priorities are out of order.
            step1.SetupGet(s => s.Priority).Returns(3);
            step2.SetupGet(s => s.Priority).Returns(2);
            step3.SetupGet(s => s.Priority).Returns(1);

            List<IQueuePipelineStep> steps = [step1.Object, step2.Object, step3.Object];
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
