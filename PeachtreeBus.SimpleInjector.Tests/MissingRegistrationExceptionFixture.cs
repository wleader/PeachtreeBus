using Moq;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using SimpleInjector;

namespace PeachtreeBus.SimpleInjector.Tests;

[TestClass]
public class MissingRegistrationExceptionFixture
{
    [TestMethod]
    public void When_New_ProprertiesAreSet()
    {
        var ex = new MissingRegistrationException(typeof(ISubscribedPipelineStep), "Help Me Please!");
        Assert.AreEqual(typeof(ISubscribedPipelineStep), ex.MissingType);
        StringAssert.Contains(ex.Message, "Help Me Please!");
    }

    [TestMethod]
    public void Given_ContainerWithoutRegistraion_When_ThrowIfNotREgisterd_Then_Throws()
    {
        var container = new Container();
        Assert.ThrowsException<MissingRegistrationException>(() =>
            MissingRegistrationException.ThrowIfNotRegistered<ISubscribedPipelineStep>(container, "Do Something different."));
    }

    [TestMethod]
    public void Given_Registration_When_ThrowIfNotRegisterd_Then_NoThrows()
    {
        var container = new Container();
        var mock = new Mock<IQueueRetryStrategy>();
        container.RegisterInstance(mock.Object);
        MissingRegistrationException.ThrowIfNotRegistered<IQueueRetryStrategy>(container);
    }
}
