using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Peachtreebus.Tests.Sagas
{
    /// <summary>
    /// A message for unit tests
    /// </summary>
    public class TestSagaMessage1 : IQueueMessage { }

    /// <summary>
    /// A message for unit tests
    /// </summary>
    public class TestSagaMessage2 : IQueueMessage { }

    /// <summary>
    /// Saga Data for tests
    /// </summary>
    public class TestSagaData { }

    /// <summary>
    /// A Saga for unit tests
    /// </summary>
    public class TestSaga : Saga<TestSagaData>,
        IHandleQueueMessage<TestSagaMessage1>
    {
        public List<Tuple<QueueContext, object>> Invocations = [];

        public override string SagaName => "TestSaga";

        public override void ConfigureMessageKeys(SagaMessageMap mapper)
        {
            mapper.Add<TestSagaMessage1>(m => "TestSagaKey");
            mapper.Add<TestSagaMessage2>(m => "TestSagaKey");
        }

        public Task Handle(QueueContext context, TestSagaMessage1 message)
        {
            Invocations.Add(new(context, message));
            return Task.CompletedTask;
        }

        public void AssertInvocations(int count)
        {
            Assert.AreEqual(count, Invocations.Count);
        }

        public void AssertInvoked<TMessage>(QueueContext context, TMessage message)
        {
            var match = Invocations.SingleOrDefault(i => ReferenceEquals(i.Item1, context) 
                && ReferenceEquals(i.Item2, message));
            Assert.IsNotNull(match);
        }
    }
}
