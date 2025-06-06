﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.ClassNames;
using PeachtreeBus.Core.Tests.Telemetry;
using PeachtreeBus.Data;
using PeachtreeBus.Exceptions;
using PeachtreeBus.Serialization;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Telemetry;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Subscriptions;

[TestClass]
public class PublishPipelineFinalStepFixture
{
    // the class under test.
    private PublishPipelineFinalStep step = default!;

    // Dependencies
    private Mock<IBusDataAccess> dataAccess = default!;
    private Mock<IMeters> meters = default!;
    private Mock<ISerializer> serializer = new();
    private Mock<ISystemClock> clock = default!;
    private BusConfiguration configuration = default!;

    // a message to send.
    private PublishContext context = default!;


    // stores the parameters to the AddMessage calls.
    private SubscribedData? PublishedMessage;
    private Topic? PublishedTopic;
    private long PublishResult = 1;

    [TestInitialize]
    public void TestInitialize()
    {
        dataAccess = new();
        meters = new();
        serializer = new();
        clock = new();
        configuration = TestData.CreateBusConfiguration();

        PublishedMessage = null;
        PublishedTopic = null;

        clock.SetupGet(c => c.UtcNow).Returns(() => TestData.Now);

        dataAccess.Setup(d => d.Publish(It.IsAny<SubscribedData>(), It.IsAny<Topic>()))
            .Callback((SubscribedData m, Topic c) =>
            {
                PublishedMessage = m;
                PublishedTopic = c;
            })
            .ReturnsAsync(() => PublishResult);

        var userMessage = TestData.CreateSubscribedUserMessage();

        context = new()
        {
            Data = new()
            {
                ValidUntil = TestData.Now.AddDays(1),
                MessageId = UniqueIdentity.New(),
                Body = new("{}"),
                Priority = 0,
                Enqueued = TestData.Now,
                NotBefore = TestData.Now,
                Headers = new()
                {
                    MessageClass = userMessage.GetType().GetClassName()
                },
                Topic = TestData.DefaultTopic,
            },
            Message = userMessage,
        };

        serializer.Setup(x => x.Serialize(context.Message, context.Message.GetType()))
            .Returns(TestData.DefaultBody);

        step = new PublishPipelineFinalStep(
            clock.Object,
            configuration,
            serializer.Object,
            dataAccess.Object,
            meters.Object);
    }

    /// <summary>
    /// Proves message cannot be null
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Invoke_ThrowsWhenContextMessageIsNull()
    {
        context.Message = null!;
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
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
        Assert.AreEqual(new("PeachtreeBus.Abstractions.Tests.TestClasses.TestSubscribedMessage, PeachtreeBus.Abstractions.Tests"),
            PublishedMessage?.Headers?.MessageClass);
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
    /// Proves Body is serialized
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Invoke_UsesBodyFromSerializer()
    {
        await step.Invoke(context, null!);
        Assert.AreEqual(TestData.DefaultBody, PublishedMessage?.Body);
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

        meters.Verify(c => c.SentMessage(count), Times.Once);
        Assert.AreEqual(count, context.RecipientCount);
    }

    /// <summary>
    /// Proves DataAccess is used
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Given_Topic_When_Invoke_Then_TopicIsUsed()
    {
        context.Data.Topic = TestData.DefaultTopic;
        await step.Invoke(context, null!);

        Assert.IsTrue(PublishedTopic.HasValue);
        Assert.AreEqual(TestData.DefaultTopic, PublishedTopic.Value);
    }

    [TestMethod]
    public async Task Given_MessageIsNotISubscribedMessage_When_Invoke_Then_ThrowsUsefulException()
    {
        context.Message = new object();
        await Assert.ThrowsExactlyAsync<TypeIsNotISubscribedMessageException>(() =>
            step.Invoke(context, null!));
    }

    [TestMethod]
    public async Task Given_Priority_When_Invoke_Then_PriorityIsSet()
    {
        context.MessagePriority = 100;
        await step.Invoke(context, null!);

        Assert.AreEqual(100, PublishedMessage?.Priority);
    }

    [TestMethod]
    public async Task Given_UserHeaders_When_Invoke_Then_UserHeadersAreUsed()
    {
        context.Data.Headers.UserHeaders = TestData.DefaultUserHeaders;
        await step.Invoke(context, null!);
        Assert.AreSame(context.UserHeaders, PublishedMessage?.Headers?.UserHeaders);
    }

    [TestMethod]
    public async Task When_Invoke_Then_ValidUntilUsesConfiguration()
    {
        var expectedValidUntil = clock.Object.UtcNow.Add(configuration.PublishConfiguration.Lifespan);

        await step.Invoke(context, null!);

        Assert.AreEqual(expectedValidUntil, PublishedMessage?.ValidUntil);
    }

    [TestMethod]
    public async Task When_Invoke_Then_Activity()
    {
        using var listener = new TestActivityListener(ActivitySources.Messaging);

        await step.Invoke(context, null);

        var activity = listener.ExpectOneCompleteActivity();
        SendActivityFixture.AssertActivity(activity, context);
    }
}
