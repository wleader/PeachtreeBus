using PeachtreeBus.Queues;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace PeachtreeBus.DependencyInjection.Testing.GetEnumerableOf;

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

public abstract class GetEnumerableOfIHandleQueueMessage_Fixture<TContainer>(ContainerBuilder<TContainer> containerBuilder)
    : GetEnumerableOfService_FixtureBase<IHandleQueueMessage<FindableQueuedMessage>, TContainer>(containerBuilder)
{
    protected override IEnumerable<Type> GetTypesToRegister() =>
    [
        typeof(FindableQueueMessageHandler1),
        typeof(FindableQueueMessageHandler2)
    ];
}
