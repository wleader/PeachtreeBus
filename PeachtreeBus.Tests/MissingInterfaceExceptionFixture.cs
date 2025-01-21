using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Tests;

[TestClass]
public class TypeIsNotIQueueMessageExceptionFixture
{
    [TestMethod]
    public void When_New_Then_PropertiesAreSet()
    {
        var ex = new TypeIsNotIQueueMessageException(typeof(object));
        Assert.IsNotNull(ex);
        Assert.AreEqual(typeof(object), ex.ClassType);
        Assert.AreEqual(typeof(IQueueMessage), ex.InterfaceType);
    }

    [TestMethod]
    public void Given_ClassWithoutInterface_When_ThrowIf_Then_Throws()
    {
        Assert.ThrowsException<TypeIsNotIQueueMessageException>(() =>
            TypeIsNotIQueueMessageException.ThrowIfMissingInterface(typeof(object)));
    }
}

[TestClass]
public class TypeIsNotISubscribedMessageExceptionFixture
{
    [TestMethod]
    public void When_New_Then_PropertiesAreSet()
    {
        var ex = new TypeIsNotISubscribedMessageException(typeof(object));
        Assert.IsNotNull(ex);
        Assert.AreEqual(typeof(object), ex.ClassType);
        Assert.AreEqual(typeof(ISubscribedMessage), ex.InterfaceType);
    }

    [TestMethod]
    public void Given_ClassWithoutInterface_When_ThrowIf_Then_Throws()
    {
        Assert.ThrowsException<TypeIsNotISubscribedMessageException>(() =>
            TypeIsNotISubscribedMessageException.ThrowIfMissingInterface(typeof(object)));
    }
}
