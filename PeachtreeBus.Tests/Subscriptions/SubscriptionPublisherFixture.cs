using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Tests.Sagas;
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

    // a message to send.
    private TestData.TestSubscribedMessage userMessage = default!;

    // stores the parameters to the AddMessage calls.
    private IPublishContext? invokedContext = default;

    [TestInitialize]
    public void TestInitialize()
    {
        pipelineInvoker = new();

        userMessage = TestData.CreateSubscribedUserMessage();

        pipelineInvoker.Setup(x => x.Invoke(It.IsAny<IPublishContext>()))
            .Callback((IPublishContext c) =>
            {
                invokedContext = c;
            });

        publisher = new SubscribedPublisher(
            pipelineInvoker.Object);
    }

    /// <summary>
    /// Proves message cannot be null
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Publish_ThrowsWhenMessageIsNull()
    {
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
            publisher.Publish(
                TestData.DefaultTopic2,
                userMessage.GetType(),
                null!,
                null));
    }

    /// <summary>
    /// Proves Type cannot be null
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Publish_ThrowsWhenTypeIsNull()
    {
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
            publisher.Publish(
                TestData.DefaultTopic2,
                null!,
                new TestSagaMessage1(),
                null));
    }

    /// <summary>
    /// Proves Header is set.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Publish_SetsContextType()
    {
        await publisher.Publish(
            TestData.DefaultTopic,
            userMessage.GetType(),
            userMessage,
            null);
        Assert.AreEqual(userMessage.GetType(), invokedContext?.Type);
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
            userMessage.GetType(),
            userMessage,
            null);
        Assert.IsNotNull(invokedContext);
        Assert.IsFalse(invokedContext.NotBefore.HasValue);
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
            userMessage.GetType(),
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
            userMessage.GetType(),
            userMessage,
            null);
        Assert.IsNotNull(invokedContext);
        Assert.AreEqual(TestData.DefaultTopic, invokedContext.Topic);
    }

    [TestMethod]
    public async Task Given_MessageIsNotISubscribedMessage_When_Publish_Then_ThrowsUsefulException()
    {
        await Assert.ThrowsExceptionAsync<TypeIsNotISubscribedMessageException>(() =>
            publisher.Publish(TestData.DefaultTopic2, typeof(object), new object(), null));
    }

    [TestMethod]
    public async Task Given_Priority_When_Publish_Then_ContextPriorityIsSet()
    {
        await publisher.Publish(
            TestData.DefaultTopic,
            userMessage.GetType(),
            userMessage,
            priority: 100);
        Assert.IsNotNull(invokedContext);
        Assert.AreEqual(100, invokedContext.Priority);
    }

    [TestMethod]
    public async Task Given_UserHeaders_When_Publish_Then_ContextUserHeadersAreSet()
    {
        await publisher.Publish(
            TestData.DefaultTopic,
            userMessage.GetType(),
            userMessage,
            userHeaders: TestData.DefaultUserHeaders);
        Assert.AreSame(TestData.DefaultUserHeaders, invokedContext?.UserHeaders);
    }

    [TestMethod]
    public async Task When_Publish_Then_PipelineInvokerIsInvoked()
    {
        await publisher.Publish(
            TestData.DefaultTopic,
            userMessage.GetType(),
            userMessage);

        pipelineInvoker.Verify(x => x.Invoke(It.IsAny<IPublishContext>()), Times.Once);
    }

    [TestMethod]
    public async Task When_Publish_Then_PipelineInvokerContextIsCorrect()
    {
        var notBefore = new DateTime(2025, 3, 16, 18, 35, 36, DateTimeKind.Utc);
        var userHeaders = new UserHeaders();
        await publisher.Publish(
            TestData.DefaultTopic,
            userMessage.GetType(),
            userMessage,
            notBefore,
            245,
            userHeaders);

        Assert.AreEqual(1, pipelineInvoker.Invocations.Count);
        var context = pipelineInvoker.Invocations[0].Arguments[0] as PublishContext;
        Assert.IsNotNull(context);
        Assert.AreSame(userMessage, context.Message);
        Assert.AreSame(userHeaders, context.UserHeaders);
        Assert.AreEqual(245, context.Priority);
        Assert.AreEqual(notBefore, context.NotBefore);
    }
}