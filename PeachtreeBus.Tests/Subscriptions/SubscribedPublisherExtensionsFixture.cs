using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Subscriptions;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Subscriptions;

[TestClass]
public class SubscribedPublisherExtensionsFixture
{
    public class TestMessage : ISubscribedMessage { }

    [TestMethod]
    public async Task When_PublishMessage_Then_ParametersArePassedToISubscribedPublisher()
    {
        var publisher = new Mock<ISubscribedPublisher>();
        var message = new TestMessage();
        var notBefore = DateTime.UtcNow;

        await publisher.Object.PublishMessage("Cat", message, notBefore, 100);

        publisher.Verify(p => p.Publish("Cat", typeof(TestMessage), message, notBefore, 100), Times.Once);
    }
}
