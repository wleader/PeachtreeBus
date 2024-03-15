using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Pipelines;
using PeachtreeBus.Queues;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Peachtreebus.Tests.Pipeline
{
    [TestClass]
    public class PipelineFixture
    {
        [TestMethod]
        public async Task Given_PipelineStepsAreProvided_When_Invoked_Then_PipelineStepsAreInvokedInOrder()
        {
            var context = new QueueContext();


            var invocations = new List<string>();

            var steps = new List<IQueuePipelineStep>()
            {
                new FakeQueuePipelineStep(3, async (c, n) =>
                {
                    invocations.Add("step3");
                    await n!.Invoke(c);
                    invocations.Add("post3");
                }),
                new FakeQueuePipelineStep(1, async (c, n) =>
                {
                    invocations.Add("step1");
                    await n!.Invoke(c);
                    invocations.Add("post1");
                }),
                new FakeQueuePipelineStep(2, async (c, n) =>
                {
                    invocations.Add("step2");
                    await n!.Invoke(c);
                    invocations.Add("post2");
                }),
            };

            var handlerStep = new FakeQueuePipelineStep(0, async (c, n) =>
            {
                invocations.Add("handler");
                Assert.IsTrue(ReferenceEquals(c, context));
                await Task.Delay(0);
            });

            var _pipeline = new Pipeline<QueueContext>();
            foreach (var step in steps.OrderBy(s => s.Priority)) { _pipeline.Add(step); }

            _pipeline.Add(handlerStep);

            await _pipeline.Invoke(context);

            // verify that all the steps were invoked in order.
            var expected = new List<string>() { "step1", "step2", "step3", "handler", "post3", "post2", "post1" };
            CollectionAssert.AreEqual(expected, invocations);
        }


    }
}
