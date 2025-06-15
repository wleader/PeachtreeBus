using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace PeachtreeBus.Abstractions.Tests.TestClasses;

[ExcludeFromCodeCoverage(Justification = "Test Class")]
public class TestQueuedMessage : IQueueMessage;

[ExcludeFromCodeCoverage(Justification = "Test Class")]
public class TestSubscribedMessage : ISubscribedMessage;

[ExcludeFromCodeCoverage(Justification = "Test Class")]
public class TestHandler : IHandleQueueMessage<TestQueuedMessage>
{
    public Task Handle(IQueueContext context, TestQueuedMessage message) =>
        throw new NotImplementedException();
}

[ExcludeFromCodeCoverage(Justification = "Test Class")]
public class TestQueuePipelineStep : IQueuePipelineStep
{
    public int Priority => 
        throw new NotImplementedException();

    public Task Invoke(IQueueContext context, Func<IQueueContext, Task> next) =>
        throw new NotImplementedException();
}