using PeachtreeBus.Queues;

namespace PeachtreeBus.ScannableAssembly;

public class QueueMessage1 : IQueueMessage;

public class QueueMessage2 : IQueueMessage;

public class HandleQueueMessage1A : IHandleQueueMessage<QueueMessage1>
{
    public Task Handle(IQueueContext context, QueueMessage1 message) => Task.CompletedTask;
}

public class HandleQueueMessage1B : IHandleQueueMessage<QueueMessage1>
{
    public Task Handle(IQueueContext context, QueueMessage1 message) => Task.CompletedTask;
}

public class HandleQueueMessage2A : IHandleQueueMessage<QueueMessage2>
{
    public Task Handle(IQueueContext context, QueueMessage2 message) => Task.CompletedTask;
}

public class HandleQueueMessage2B : IHandleQueueMessage<QueueMessage2>
{
    public Task Handle(IQueueContext context, QueueMessage2 message) => Task.CompletedTask;
}