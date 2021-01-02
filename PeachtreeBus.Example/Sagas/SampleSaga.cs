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

        public SampleSaga(ILog<SampleSaga> log)
        {
            _log = log;
        }

        public override void ConfigureMessageKeys(SagaMessageMap mapper)
        {
            mapper.Add<SampleSagaStart>(m => m.SagaId.ToString());
            mapper.Add<SampleDistributedTaskResponse>(m => m.SagaId.ToString());
        }

        public Task Handle(MessageContext context, SampleDistributedTaskResponse message)
        {
            _log.Info($"Distributed Task Complete: {message.A} {message.Operation} {message.B} = {message.Result}");
            Data.PendingTasks--;
            _log.Info($"{Data.PendingTasks} Tasks Remaining.");

            if (Data.PendingTasks == 0)
            {
                context.Send(new SampleSagaComplete { SagaId = Data.SagaId });
                SagaComplete = true;
            }
            return Task.CompletedTask;
        }

        public Task Handle(MessageContext context, SampleSagaStart message)
        {
            _log.Info("Distributing Tasks.");

            Data.SagaId = message.SagaId;
            Data.PendingTasks = 10;

            for (var i = 0; i < 10; i++)
            {
                context.Send(new SampleDistributedTaskRequest
                {
                    SagaId = message.SagaId,
                    A = i * 3,
                    B = i * 4,
                    Operation = "+"
                });
            }

            return Task.CompletedTask;
        }
    }
}
