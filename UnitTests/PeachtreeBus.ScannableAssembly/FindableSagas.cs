using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;

namespace PeachtreeBus.ScannableAssembly;

public class SagaMessage1 : IQueueMessage;

public class SagaMessage2 : IQueueMessage;

public class Saga1
    : Saga<Saga1.SagaData>
    , IHandleSagaStartMessage<SagaMessage1>
    , IHandleQueueMessage<SagaMessage2>
{
    public class SagaData;
    public override SagaName SagaName => new("Saga1");
    public override void ConfigureMessageKeys(ISagaMessageMap mapper) { }
    public Task Handle(IQueueContext context, SagaMessage1 message) => Task.CompletedTask;
    public Task Handle(IQueueContext context, SagaMessage2 message) => Task.CompletedTask;
}

public class Saga2
    : Saga<Saga2.SagaData>
    , IHandleSagaStartMessage<SagaMessage1>
    , IHandleQueueMessage<SagaMessage2>
{
    public class SagaData;
    public override SagaName SagaName => new("Saga1");
    public override void ConfigureMessageKeys(ISagaMessageMap mapper) { }
    public Task Handle(IQueueContext context, SagaMessage1 message) => Task.CompletedTask;
    public Task Handle(IQueueContext context, SagaMessage2 message) => Task.CompletedTask;
}