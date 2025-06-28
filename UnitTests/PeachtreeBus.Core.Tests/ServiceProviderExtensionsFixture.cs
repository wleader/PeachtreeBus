using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Queues;
using System.Collections.Generic;
using System.Linq;

namespace PeachtreeBus.Core.Tests;

[TestClass]
public class ServiceProviderExtensionsFixture
{
    private class FindableMessage : IQueueMessage;
    private readonly FakeServiceProviderAccessor _accessor = new();

    [TestInitialize]
    public void Initialize()
    {
        _accessor.Reset();
    }

    [TestMethod]
    public void Given_ScopeReturnsNull_When_FindHandlers_Then_EmptyResult()
    {
        _accessor.SetupService<IEnumerable<IHandleQueueMessage<FindableMessage>>>(() => null!);
        var actual = _accessor.GetServices<IHandleQueueMessage<FindableMessage>>();
        Assert.IsNotNull(actual);
        Assert.IsFalse(actual.Any());
    }

    [TestMethod]
    public void Given_ScopeReturnsObject_When_FindHandlers_Then_Result()
    {
        IEnumerable<IHandleQueueMessage<FindableMessage>> expected = [];
        _accessor.SetupService(() => expected);
        var actual = _accessor.GetServices<IHandleQueueMessage<FindableMessage>>();
        Assert.AreSame(expected, actual);
    }
}
