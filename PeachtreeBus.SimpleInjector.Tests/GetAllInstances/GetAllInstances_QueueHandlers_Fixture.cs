using PeachtreeBus.Queues;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using static PeachtreeBus.SimpleInjector.Tests.GetAllInstances.GetAllInstances_QueueHandlers_Fixture;

namespace PeachtreeBus.SimpleInjector.Tests.GetAllInstances;

[TestClass]
public class GetAllInstances_QueueHandlers_Fixture
    : GetAllInstances_Base_Fixture<IHandleQueueMessage<FindableQueuedMessage>>
{
    public class FindableQueuedMessage : IQueueMessage;

    [ExcludeFromCodeCoverage]
    public class FindableQueueMessageHandler1 : IHandleQueueMessage<FindableQueuedMessage>
    {
        public Task Handle(IQueueContext context, FindableQueuedMessage message) => throw new NotImplementedException();
    }

    [ExcludeFromCodeCoverage]
    public class FindableQueueMessageHandler2 : IHandleQueueMessage<FindableQueuedMessage>
    {
        public Task Handle(IQueueContext context, FindableQueuedMessage message) => throw new NotImplementedException();
    }

    protected override IEnumerable<Type> GetTypesToRegister() =>
    [
        typeof(FindableQueueMessageHandler1),
        typeof(FindableQueueMessageHandler2)
    ];
}
