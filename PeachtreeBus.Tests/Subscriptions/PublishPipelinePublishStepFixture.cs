using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Interfaces;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Tests.Fakes;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Subscriptions;

[TestClass]
public class PublishPipelinePublishStepFixture
{
    // the class under test.
    private PublishPipelinePublishStep step = default!;

    // Dependencies
    private Mock<IBusDataAccess> dataAccess = default!;
    private Mock<IPerfCounters> counters = default!;
    private FakeSerializer serializer = default!;
    private Mock<ISystemClock> clock = default!;
    private BusConfiguration configuration = default!;

    // a message to send.
    private PublishContext context = default!;


    // stores the parameters to the AddMessage calls.
    private SubscribedMessage? PublishedMessage;
    private Topic? PublishedTopic;
    private long PublishResult = 1;

    [TestInitialize]
    public void TestInitialize()
    {
        dataAccess = new();
        counters = new();
        serializer = new();
        clock = new();
        configuration = TestData.CreateBusConfiguration();

        PublishedMessage = null;
        PublishedTopic = null;

        clock.SetupGet(c => c.UtcNow).Returns(() => TestData.Now);

        dataAccess.Setup(d => d.Publish(It.IsAny<SubscribedMessage>(), It.IsAny<Topic>()))
            .Callback((SubscribedMessage m, Topic c) =>
            {
                PublishedMessage = m;
                PublishedTopic = c;
            })
            .ReturnsAsync(() => PublishResult);

        context = new()
        {
            Message = TestData.CreateSubscribedUserMessage(),
            Type = typeof(TestData.TestSubscribedMessage),
            Topic = TestData.DefaultTopic,
        };

        step = new PublishPipelinePublishStep(
            clock.Object,
            configuration,
            serializer.Object,
            dataAccess.Object,
            counters.Object);
    }

    /// <summary>
    /// Proves message cannot be null
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Invoke_ThrowsWhenContextMessageIsNull()
    {
        context.Message = null!;
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
            step.Invoke(context, null!));
    }

    /// <summary>
    /// Proves Type cannot be null
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Invoke_ThrowsWhenContextTypeIsNull()
    {
        context.Type = null!;
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
            step.Invoke(context, null!));
    }

    /// <summary>
    /// Proves Header is set.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Invoke_SetsTypeOnHeaders()
    {
        await step.Invoke(context, null!);
        Assert.AreEqual(1, serializer.SerializedHeaders.Count);
        Assert.AreEqual("PeachtreeBus.Tests.TestData+TestSubscribedMessage, PeachtreeBus.Tests",
            serializer.SerializedHeaders[0].MessageClass);
    }

    /// <summary>
    /// Proves NotBefore is defaulted
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Invoke_DefaultsNotBeforeToUtcNow()
    {
        context.NotBefore = null;
        await step.Invoke(context, null!);

        Assert.AreEqual(clock.Object.UtcNow, PublishedMessage?.NotBefore);
    }

    /// <summary>
    /// Proves NotBefore is used
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Invoke_UsesProvidedNotBefore()
    {
        UtcDateTime notBefore = DateTime.UtcNow;
        context.NotBefore = notBefore;

        await step.Invoke(context, null!);

        Assert.AreEqual(notBefore, PublishedMessage?.NotBefore);
    }

    /// <summary>
    /// Proves Enqueued is set.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Invoke_SetsEnqueuedToUtcNow()
    {
        await step.Invoke(context, null!);
        Assert.AreEqual(clock.Object.UtcNow, PublishedMessage?.Enqueued);
    }

    /// <summary>
    /// Proves that completed defaults to null
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Invoke_SetsCompletedToNull()
    {
        await step.Invoke(context, null!);

        Assert.IsNotNull(PublishedMessage);
        Assert.IsNull(PublishedMessage.Completed);
    }

    /// <summary>
    /// Proves that failed defaults to null.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Invoke_SetsFailedToNull()
    {
        await step.Invoke(context, null!);

        Assert.IsNotNull(PublishedMessage);
        Assert.IsNull(PublishedMessage.Failed);
    }

    /// <summary>
    /// proves retries defaults to zero.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Invoke_SetsRetriesToZero()
    {
        await step.Invoke(context, null!);

        Assert.IsNotNull(PublishedMessage);
        Assert.AreEqual(0, PublishedMessage.Retries);
    }

    /// <summary>
    /// Proves headers are serialized.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Invoke_UsesHeadersFromSerializer()
    {
        await step.Invoke(context, null!);

        Assert.AreEqual(serializer.SerializeHeadersResult, PublishedMessage?.Headers);
    }

    /// <summary>
    /// Proves Body is serialized
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Invoke_UsesBodyFromSerializer()
    {
        await step.Invoke(context, null!);

        Assert.AreEqual(serializer.SerializeMessageResult, PublishedMessage?.Body);
    }

    /// <summary>
    /// Proves Perf counters are used.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(10)]
    [DataRow(long.MaxValue)]
    public async Task Invoke_SetsContextReciepentCount(long count)
    {
        PublishResult = count;

        await step.Invoke(context, null!);

        counters.Verify(c => c.PublishMessage(count), Times.Once);
        Assert.AreEqual(count, context.RecipientCount);
    }

    /// <summary>
    /// Proves DataAccess is used
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Given_Topic_When_Invoke_Then_TopicIsUsed()
    {
        context.Topic = TestData.DefaultTopic;
        await step.Invoke(context, null!);

        Assert.IsTrue(PublishedTopic.HasValue);
        Assert.AreEqual(TestData.DefaultTopic, PublishedTopic.Value);
    }

    [TestMethod]
    public async Task Given_MessageIsNotISubscribedMessage_When_Invoke_Then_ThrowsUsefulException()
    {
        context.Message = new object();
        context.Type = typeof(object);
        await Assert.ThrowsExceptionAsync<TypeIsNotISubscribedMessageException>(() =>
            step.Invoke(context, null!));
    }

    [TestMethod]
    public async Task Given_Priority_When_Invoke_Then_PriorityIsSet()
    {
        context.Priority = 100;
        await step.Invoke(context, null!);

        Assert.AreEqual(100, PublishedMessage?.Priority);
    }

    [TestMethod]
    public async Task Given_UserHeaders_When_Invoke_Then_UserHeadersAreUsed()
    {
        context.UserHeaders = TestData.DefaultUserHeaders;
        await step.Invoke(context, null!);

        Assert.AreEqual(1, serializer.SerializedHeaders.Count);
        Assert.AreSame(TestData.DefaultUserHeaders, serializer.SerializedHeaders[0].UserHeaders);
    }

    [TestMethod]
    public async Task When_Invoke_Then_ValidUntilUsesConfiguration()
    {
        var expectedValidUntil = clock.Object.UtcNow.Add(configuration.PublishConfiguration.Lifespan);

        await step.Invoke(context, null!);

        Assert.AreEqual(expectedValidUntil, PublishedMessage?.ValidUntil);
    }
}
