using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace PeachtreeBus.DependencyInjection.Testing;

public class QueueMessage : IQueueMessage;

[ExcludeFromCodeCoverage(Justification = "Non-Shipping Test class")]
public class QueueHandler : IHandleQueueMessage<QueueMessage>
{
    public Task Handle(IQueueContext context, QueueMessage message) => Task.CompletedTask;
}

public class SubscribedMessage : ISubscribedMessage;

[ExcludeFromCodeCoverage(Justification = "Non-Shipping Test class")]
public class SubscribedHandler : IHandleSubscribedMessage<SubscribedMessage>
{
    public Task Handle(ISubscribedContext context, SubscribedMessage message) => Task.CompletedTask;
}

[ExcludeFromCodeCoverage(Justification = "Non-Shipping Test class")]
public class PublishPipelineStep : IPublishPipelineStep
{
    public int Priority => 0;
    public Task Invoke(IPublishContext context, Func<IPublishContext, Task> next) => Task.CompletedTask;
}

[ExcludeFromCodeCoverage(Justification = "Non-Shipping Test class")]
public class SendPipelineStep : ISendPipelineStep
{
    public int Priority => 0;
    public Task Invoke(ISendContext context, Func<ISendContext, Task> next) => Task.CompletedTask;
}

[ExcludeFromCodeCoverage(Justification = "Non-Shipping Test class")]
public class QueuePipelineStep : IQueuePipelineStep
{
    public int Priority => 0;
    public Task Invoke(IQueueContext context, Func<IQueueContext, Task> next) => Task.CompletedTask;
}

[ExcludeFromCodeCoverage(Justification = "Non-Shipping Test class")]
public class SubscribedPipelineStep : ISubscribedPipelineStep
{
    public int Priority => 0;
    public Task Invoke(ISubscribedContext context, Func<ISubscribedContext, Task> next) => Task.CompletedTask;
}

[ExcludeFromCodeCoverage(Justification = "Non-Shipping Test class")]
public class RunOnStartup : IRunOnStartup
{
    public Task Run() => Task.CompletedTask;
}