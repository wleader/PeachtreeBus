using PeachtreeBus.Example.Data;
using PeachtreeBus.Example.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PeachtreeBus.Example.Sagas
{

    public class SampleSagaData
    {
        public int SagaId { get; set; }
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
            mapper.Add<SampleSagaStart>(m => m.SagaId.ToString());
            mapper.Add<SampleDistributedTaskResponse>(m => m.SagaId.ToString());
        }

        public async Task Handle(MessageContext context, SampleDistributedTaskResponse message)
        {
            _log.Info($"Distributed Task Complete: {message.A} {message.Operation} {message.B} = {message.Result}");
            Data.PendingTasks--;
            _log.Info($"{Data.PendingTasks} Tasks Remaining.");

            if (Data.PendingTasks == 0)
            {
                context.Send(new SampleSagaComplete { SagaId = Data.SagaId });
                SagaComplete = true;
            }

            await _dataAccess.Audit($"Pending Tasks {Data.PendingTasks}");
        }

        public async Task Handle(MessageContext context, SampleSagaStart message)
        {
            await _dataAccess.Audit("Starting Saga.");
            _log.Info($"Distributing Tasks for SagaId {message.SagaId}");

            Data.SagaId = message.SagaId;
            Data.PendingTasks = 100;

            for (var i = 0; i < Data.PendingTasks; i++)
            {
                context.Send(new SampleDistributedTaskRequest
                {
                    SagaId = message.SagaId,
                    A = i * 3,
                    B = i * 4,
                    Operation = "+"
                });
            }
        }
    }
}
