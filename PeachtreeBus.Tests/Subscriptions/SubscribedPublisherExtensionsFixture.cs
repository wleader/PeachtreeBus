using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Subscriptions;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Subscriptions;

[TestClass]
public class SubscribedPublisherExtensionsFixture
{
    private Mock<ISubscribedPublisher> _publisher = default!;

    [TestInitialize]
    public void Initialize()
    {
        _publisher = new();
    }

    [TestMethod]
    public async Task When_PublishMessage_Then_ParametersArePassedToISubscribedPublisher()
    {
        var message = TestData.CreateQueueUserMessage();
        var notBefore = DateTime.UtcNow;

        await _publisher.Object.PublishMessage(
            TestData.DefaultCategory,
            message,
            notBefore,
            100,
            TestData.DefaultUserHeaders);

        _publisher.Verify(p => p.Publish(
            TestData.DefaultCategory,
            message.GetType(),
            message,
            notBefore,
            100,
            TestData.DefaultUserHeaders),
            Times.Once);
    }
}
