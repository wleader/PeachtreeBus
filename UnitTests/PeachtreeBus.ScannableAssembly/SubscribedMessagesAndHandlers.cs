using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.ScannableAssembly;

public class SubscribedMessage1 : ISubscribedMessage;

public class SubscribedMessage2 : ISubscribedMessage;

public class HandleSubscribedMessage1A : IHandleSubscribedMessage<SubscribedMessage1>
{
    public Task Handle(ISubscribedContext context, SubscribedMessage1 message) => Task.CompletedTask;
}

public class HandleSubscribedMessage1B : IHandleSubscribedMessage<SubscribedMessage1>
{
    public Task Handle(ISubscribedContext context, SubscribedMessage1 message) => Task.CompletedTask;
}

public class HandleSubscribedMessage2A : IHandleSubscribedMessage<SubscribedMessage2>
{
    public Task Handle(ISubscribedContext context, SubscribedMessage2 message) => Task.CompletedTask;
}

public class HandleSubscribedMessage2B : IHandleSubscribedMessage<SubscribedMessage2>
{
    public Task Handle(ISubscribedContext context, SubscribedMessage2 message) => Task.CompletedTask;
}