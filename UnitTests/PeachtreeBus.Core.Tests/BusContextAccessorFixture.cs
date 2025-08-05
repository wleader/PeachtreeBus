using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.ClassNames;
using System;
using System.Diagnostics.CodeAnalysis;

namespace PeachtreeBus.Core.Tests;

[TestClass]
public class BusContextAccessorFixture
{
    private readonly BusContextAccessor _accessor = new();

    [ExcludeFromCodeCoverage(Justification = "Non Shipping Test Class")]
    public class InvalidContext : IContext
    {
        public IServiceProvider? ServiceProvider => default;
        public object Message => default!;
        public ClassName MessageClass => default!;
    }

    [TestMethod]
    public void Given_Accessor_When_SetQueueContext_Then_QueueContextSet()
    {
        var context = TestData.CreateQueueContext();
        BusContextAccessor.Set(context);
        Assert.AreSame(context, _accessor.QueueContext);
        Assert.AreSame(context, _accessor.IncomingContext);
        Assert.IsNull(_accessor.SubscribedContext);
    }

    [TestMethod]
    public void Given_Accessor_When_SetSubscribedContext_Then_SubscribedContextSet()
    {
        var context = TestData.CreateSubscribedContext();
        BusContextAccessor.Set(context);
        Assert.AreSame(context, _accessor.SubscribedContext);
        Assert.AreSame(context, _accessor.IncomingContext);
        Assert.IsNull(_accessor.QueueContext);
    }

    [TestMethod]
    public void Given_Accessor_When_SetSendContext_Then_SendContextSet()
    {
        var context = TestData.CreateSendContext();
        BusContextAccessor.Set(context);
        Assert.AreSame(context, _accessor.SendContext);
        Assert.AreSame(context, _accessor.OutgoingContext);
        Assert.IsNull(_accessor.PublishContext);
    }

    [TestMethod]
    public void Given_Accessor_When_SetPublishContext_Then_PublishContextSet()
    {
        var context = TestData.CreatePublishContext();
        BusContextAccessor.Set(context);
        Assert.AreSame(context, _accessor.PublishContext);
        Assert.AreSame(context, _accessor.OutgoingContext);
        Assert.IsNull(_accessor.SendContext);
    }

    [TestMethod]
    public void Given_UnsupportedContext_When_Set_Then_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            BusContextAccessor.Set(new InvalidContext()));
    }
}
