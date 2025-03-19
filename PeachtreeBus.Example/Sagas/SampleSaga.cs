using Microsoft.Extensions.Logging;
using PeachtreeBus.Example.Data;
using PeachtreeBus.Example.Messages;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using System.Threading.Tasks;

namespace PeachtreeBus.Example.Sagas
{
    /// <summary>
    /// An example of SagaData that will be stored and read each time a saga message is handled.
    /// </summary>
    public class SampleSagaData
    {
        public int PendingTasks { get; set; }
    }

    /// <summary>
    /// An example of a saga.
    /// </summary>
    public class SampleSaga(
        ILogger<SampleSaga> log,
        IExampleDataAccess dataAccess,
        IQueueWriter queueWriter)
        : Saga<SampleSagaData>
        , IHandleSagaStartMessage<SampleSagaStart>
        , IHandleQueueMessage<SampleDistributedTaskResponse>
    {
        private readonly ILogger _log = log;
        private readonly IExampleDataAccess _dataAccess = dataAccess;
        private readonly IQueueWriter _queueWriter = queueWriter;

        private static readonly SagaName _sagaName = new("SampleSaga");

        /// <summary>
        /// A saga must provide a saga name, This value is used to ensure that each saga 
        /// has its own data table in database. This value must be unique for each of your sagas.
        /// </summary>
        public override SagaName SagaName => _sagaName;

        /// <summary>
        /// Describes how to find the correct row in the saga data table for any given message being handled.
        /// </summary>
        /// <param name="mapper"></param>
        public override void ConfigureMessageKeys(ISagaMessageMap mapper)
        {
            // each of these maps should return the same string value for a given instance of the saga.
            mapper.Add<SampleSagaStart>(m => new(m.AppId.ToString()));
            mapper.Add<SampleDistributedTaskResponse>(m => new(m.AppId.ToString()));
        }

        /// <summary>
        /// Handles a response for the distributed task.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task Handle(IQueueContext context, SampleDistributedTaskResponse message)
        {
            _log.DistributedTaskComplete(message.Operation, message.A, message.B, message.Result);

            // update our saga data, keeping track of how much work is remaining.
            Data.PendingTasks--;
            _log.PendingTasksRemaining(Data.PendingTasks);

            if (Data.PendingTasks == 0)
            {
                // all tasks are complete, so we can inform the application
                // that this saga has finished.
                _log.CompletingSaga(message.AppId);
                await _queueWriter.WriteMessage(context.SourceQueue,
                    new SampleSagaComplete { AppId = message.AppId });
                SagaComplete = true;
            }
            else
            {
                // there is more work to do, so dispatch another distributed task.
                _log.DistributingMoreWork(message.AppId);
                await _queueWriter.WriteMessage(context.SourceQueue,
                    new SampleDistributedTaskRequest
                    {
                        AppId = message.AppId,
                        B = (Data.PendingTasks - 1) * 3,
                        A = (Data.PendingTasks - 1) * 4,
                        Operation = "+"
                    });
            }

            // demonstrate interaction with our application database.
            await _dataAccess.Audit($"Pending Tasks {Data.PendingTasks}");
        }

        /// <summary>
        /// A message handler that starts the sample saga.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task Handle(IQueueContext context, SampleSagaStart message)
        {
            _log.StartingTasks(message.AppId);

            // record in our saga data how many distributed tasks that need to be compelted before
            // the saga is regarded as complete.
            Data.PendingTasks = 10;

            // send a distributed task out to be worked.
            await _queueWriter.WriteMessage(context.SourceQueue,
                new SampleDistributedTaskRequest
                {
                    AppId = message.AppId,
                    A = (Data.PendingTasks - 1) * 3,
                    B = (Data.PendingTasks - 1) * 4,
                    Operation = "+"
                });

            // interact with our application specific database.
            await _dataAccess.Audit("Saga Started.");
        }
    }
}
