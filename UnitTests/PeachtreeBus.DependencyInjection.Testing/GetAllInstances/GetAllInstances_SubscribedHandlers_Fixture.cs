using PeachtreeBus.Subscriptions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace PeachtreeBus.DependencyInjection.Testing.GetAllInstances;

public class FindableSubscribedMessage : ISubscribedMessage;

[ExcludeFromCodeCoverage]
public class FindableSubscribedMessageHandler1 : IHandleSubscribedMessage<FindableSubscribedMessage>
{
    public Task Handle(ISubscribedContext context, FindableSubscribedMessage message) => throw new NotImplementedException();
}

[ExcludeFromCodeCoverage]
public class FindableSubscribedMessageHandler2 : IHandleSubscribedMessage<FindableSubscribedMessage>
{
    public Task Handle(ISubscribedContext context, FindableSubscribedMessage message) => throw new NotImplementedException();
}

public abstract class GetAllInstances_SubscribedHandlers_Fixture<TContainer>(ContainerBuilder<TContainer> containerBuilder)
     : GetAllInstances_Base_Fixture<IHandleSubscribedMessage<FindableSubscribedMessage>, TContainer>(containerBuilder)
{
    protected override IEnumerable<Type> GetTypesToRegister() =>
    [
        typeof(FindableSubscribedMessageHandler1),
        typeof(FindableSubscribedMessageHandler2)
    ];
}
