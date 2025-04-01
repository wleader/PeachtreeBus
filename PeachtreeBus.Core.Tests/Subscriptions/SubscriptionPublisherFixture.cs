using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Abstractions.Tests.TestClasses;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Subscriptions;

/// <summary>
/// Proves the behavior of SubscriptionPublisher
/// </summary>
[TestClass]
public class SubscriptionPublisherFixture
{
    // the class under test.
    private SubscribedPublisher publisher = default!;

    // Dependencies
    private Mock<IPublishPipelineInvoker> pipelineInvoker = default!;
    private readonly Mock<ISystemClock> clock = new();

    // a message to send.
    private TestSubscribedMessage userMessage = default!;

    // stores the parameters to the AddMessage calls.
    private IPublishContext? invokedContext = default;

    [TestInitialize]
    public void TestInitialize()
    {
        pipelineInvoker = new();
        clock.Reset();

        clock.SetupGet(c => c.UtcNow).Returns(TestData.Now);

        userMessage = TestData.CreateSubscribedUserMessage();

        pipelineInvoker.Setup(x => x.Invoke(It.IsAny<PublishContext>()))
            .Callback((IPublishContext c) =>
            {
                invokedContext = c;
            });

        publisher = new SubscribedPublisher(
            clock.Object,
            pipelineInvoker.Object);
    }

    /// <summary>
    /// Proves message cannot be null
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Publish_ThrowsWhenMessageIsNull()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            publisher.Publish(
                TestData.DefaultTopic2,
                null!,
                null));
    }

    /// <summary>
    /// Proves NotBefore is defaulted
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Publish_PassesNullNotBefore()
    {
        await publisher.Publish(
            TestData.DefaultTopic,
            userMessage,
            null);
        Assert.IsNotNull(invokedContext);
        Assert.AreEqual(TestData.Now, invokedContext.NotBefore);
    }

    /// <summary>
    /// Proves NotBefore is used
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Publish_UsesProvidedNotBefore()
    {
        UtcDateTime notBefore = DateTime.UtcNow;
        await publisher.Publish(
            TestData.DefaultTopic,
            userMessage,
            notBefore);

        Assert.AreEqual(notBefore, invokedContext?.NotBefore);
    }

    /// <summary>
    /// Proves DataAccess is used
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Given_Topic_When_Publish_Then_ContextTopicIsSet()
    {
        await publisher.Publish(
            TestData.DefaultTopic,
            userMessage,
            null);
        Assert.IsNotNull(invokedContext);
        Assert.AreEqual(TestData.DefaultTopic, invokedContext.Topic);
    }

    [TestMethod]
    public async Task Given_Priority_When_Publish_Then_ContextPriorityIsSet()
    {
        await publisher.Publish(
            TestData.DefaultTopic,
            userMessage,
            priority: 100);
        Assert.IsNotNull(invokedContext);
        Assert.AreEqual(100, invokedContext.MessagePriority);
    }

    [TestMethod]
    public async Task Given_UserHeaders_When_Publish_Then_ContextUserHeadersAreSet()
    {
        await publisher.Publish(
            TestData.DefaultTopic,
            userMessage,
            userHeaders: TestData.DefaultUserHeaders);
        Assert.AreSame(TestData.DefaultUserHeaders, invokedContext?.UserHeaders);
    }

    [TestMethod]
    public async Task When_Publish_Then_PipelineInvokerIsInvoked()
    {
        await publisher.Publish(
            TestData.DefaultTopic,
            userMessage);

        pipelineInvoker.Verify(x => x.Invoke(It.IsAny<PublishContext>()), Times.Once);
    }

    [TestMethod]
    public async Task When_Publish_Then_PipelineInvokerContextIsCorrect()
    {
        UtcDateTime notBefore = new DateTime(2025, 3, 16, 18, 35, 36, DateTimeKind.Utc);
        var userHeaders = new UserHeaders();
        await publisher.Publish(
            TestData.DefaultTopic,
            userMessage,
            notBefore,
            245,
            userHeaders);

        Assert.AreEqual(1, pipelineInvoker.Invocations.Count);
        var context = pipelineInvoker.Invocations[0].Arguments[0] as PublishContext;
        Assert.IsNotNull(context);
        Assert.AreSame(userMessage, context.Message);
        Assert.AreSame(userHeaders, context.UserHeaders);
        Assert.AreEqual(245, context.MessagePriority);
        Assert.AreEqual(notBefore, context.NotBefore);
    }
}