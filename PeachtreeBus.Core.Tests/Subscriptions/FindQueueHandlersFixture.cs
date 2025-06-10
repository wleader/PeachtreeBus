using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Exceptions;
using PeachtreeBus.Subscriptions;
using System.Collections.Generic;

namespace PeachtreeBus.Core.Tests.Subscriptions;

[TestClass]
public class FindSubscribedHandlersFixture
{
    private class FindableMessage : ISubscribedMessage;

    private FindSubscribedHandlers _finder = default!;
    private readonly Mock<IWrappedScope> _scope = new();

    [TestInitialize]
    public void Initialize()
    {
        _scope.Reset();

        _finder = new(
            _scope.Object);
    }

    [TestMethod]
    public void Given_ScopeReturnsNull_When_FindHandlers_Then_Throws()
    {
        _scope.Setup(s => s.GetAllInstances<IHandleSubscribedMessage<FindableMessage>>())
            .Returns((IEnumerable<IHandleSubscribedMessage<FindableMessage>>)null!);
        var thrown = Assert.ThrowsExactly<IncorrectImplementationException>(() =>
            _ = _finder.FindHandlers<FindableMessage>());
        Assert.IsFalse(string.IsNullOrWhiteSpace(thrown.Message));
        Assert.AreEqual(typeof(IWrappedScope), thrown.InterfaceType);
        Assert.AreEqual(_scope.Object.GetType(), thrown.ClassType);
    }

    [TestMethod]
    public void Given_ScopeReturnsObject_When_FindHandlers_Then_Result()
    {
        IEnumerable<IHandleSubscribedMessage<FindableMessage>> expected = [];
        _scope.Setup(s => s.GetAllInstances<IHandleSubscribedMessage<FindableMessage>>())
            .Returns(expected!);
        var actual = _finder.FindHandlers<FindableMessage>();
        Assert.AreSame(expected, actual);
    }
}
