using PeachtreeBus.Example.Data;
using PeachtreeBus.Example.Messages;
using System.Threading.Tasks;

namespace PeachtreeBus.Example.Sagas
{

    public class SampleSagaData
    {
        public int PendingTasks { get; set; }
    }

    public class SampleSaga : Saga<SampleSagaData>,
        IHandleSagaStartMessage<SampleSagaStart>,
        IHandleMessage<SampleDistributedTaskResponse>
    {
        private readonly ILog _log;
        private readonly IExampleDataAccess _dataAccess;

        public SampleSaga(ILog<SampleSaga> log, IExampleDataAccess dataAccess)
        {
            _log = log;
            _dataAccess = dataAccess;
        }

        public override string SagaName => "SampleSaga";

        public override void ConfigureMessageKeys(SagaMessageMap mapper)
        {
            mapper.Add<SampleSagaStart>(m => m.AppId.ToString());
            mapper.Add<SampleDistributedTaskResponse>(m => m.AppId.ToString());
        }

        public Task Handle(MessageContext context, SampleDistributedTaskResponse message)
        {
            _log.Info($"Distributed Task Complete: {message.A} {message.Operation} {message.B} = {message.Result}");
            Data.PendingTasks--;
            _log.Info($"{Data.PendingTasks} Tasks Remaining.");

            if (Data.PendingTasks == 0)
            {
                _log.Info($"Completing SagaId {message.AppId}");
                context.Send(new SampleSagaComplete { AppId = message.AppId });
                SagaComplete = true;
            }
            else
            {
                _log.Info($"Distributing more work for SagaId {message.AppId}");
                context.Send(new SampleDistributedTaskRequest
                {
                    AppId = message.AppId,
                    B = (Data.PendingTasks - 1) * 3,
                    A = (Data.PendingTasks - 1) * 4,
                    Operation = "+"
                });
            }

            return _dataAccess.Audit($"Pending Tasks {Data.PendingTasks}");
        }

        public Task Handle(MessageContext context, SampleSagaStart message)
        {
            _log.Info($"Starting Tasks for SagaId {message.AppId}");

            Data.PendingTasks = 100;

            context.Send(new SampleDistributedTaskRequest
            {
                AppId = message.AppId,
                A = (Data.PendingTasks - 1) * 3,
                B = (Data.PendingTasks - 1) * 4,
                Operation = "+"
            });

            return _dataAccess.Audit("Saga Started.");
        }
    }
}
