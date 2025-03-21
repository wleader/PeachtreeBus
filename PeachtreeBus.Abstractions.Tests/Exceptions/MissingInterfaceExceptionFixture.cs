using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Exceptions;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Abstractions.Tests.Exceptions;

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

    [TestMethod]
    public void Given_ClassWithInterface_When_ThrowIf_Then_Returns()
    {
        TypeIsNotIQueueMessageException.ThrowIfMissingInterface(
            typeof(AbstractionsTestData.TestQueuedMessage));
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

    [TestMethod]
    public void Given_ClassWithInterface_When_ThrowIf_Then_Returns()
    {
        TypeIsNotISubscribedMessageException.ThrowIfMissingInterface(
            typeof(AbstractionsTestData.TestSubscribedMessage));
    }
}
