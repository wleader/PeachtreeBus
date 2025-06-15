using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace PeachtreeBus.SimpleInjector.Tests;

public class TestSubscribedMessage : ISubscribedMessage;

[ExcludeFromCodeCoverage] // this is just something for the extensions to find.
public class TestSubscribedHandler : IHandleSubscribedMessage<TestSubscribedMessage>
{
    public Task Handle(ISubscribedContext context, TestSubscribedMessage message)
    {
        return Task.CompletedTask;
    }
}

public class TestQueueMessage : IQueueMessage;

[ExcludeFromCodeCoverage] // this is just something for the extensions to find.
public class TestQueueHandler : IHandleQueueMessage<TestQueueMessage>
{
    public Task Handle(IQueueContext context, TestQueueMessage message)
    {
        return Task.CompletedTask;
    }
}
