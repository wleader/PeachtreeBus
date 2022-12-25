using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using System.Threading.Tasks;

namespace Peachtreebus.Tests.Sagas
{
    /// <summary>
    /// A message for unit tests
    /// </summary>
    public class TestSagaMessage1 : IQueueMessage
    {

    }

    /// <summary>
    /// A message for unit tests
    /// </summary>
    public class TestSagaMessage2 : IQueueMessage
    {

    }

    /// <summary>
    /// Saga Data for tests
    /// </summary>
    public class TestSagaData
    {

    }

    /// <summary>
    /// A Saga for unit tests
    /// </summary>
    public class TestSaga : Saga<TestSagaData>,
        IHandleQueueMessage<TestSagaMessage1>
    {
        public override string SagaName => "TestSaga";

        public override void ConfigureMessageKeys(SagaMessageMap mapper)
        {
            mapper.Add<TestSagaMessage1>(m => "TestSagaKey");
            mapper.Add<TestSagaMessage2>(m => "TestSagaKey");
        }

        public Task Handle(QueueContext context, TestSagaMessage1 message)
        {
            // do nothing.
            return Task.CompletedTask;
        }
    }
}
