using PeachtreeBus.Subscriptions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using static PeachtreeBus.SimpleInjector.Tests.GetAllInstances.GetAllInstances_SubscribedHandlers_Fixture;

namespace PeachtreeBus.SimpleInjector.Tests.GetAllInstances;

[TestClass]
public class GetAllInstances_SubscribedHandlers_Fixture
     : GetAllInstances_Base_Fixture<IHandleSubscribedMessage<FindableSubscribedMessage>>
{
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

    protected override IEnumerable<Type> GetTypesToRegister() =>
    [
        typeof(FindableSubscribedMessageHandler1),
        typeof(FindableSubscribedMessageHandler2)
    ];
}
