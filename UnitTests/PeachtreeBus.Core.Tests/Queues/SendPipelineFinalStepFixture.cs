using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.ClassNames;
using PeachtreeBus.Core.Tests.Telemetry;
using PeachtreeBus.Data;
using PeachtreeBus.Exceptions;
using PeachtreeBus.Queues;
using PeachtreeBus.Serialization;
using PeachtreeBus.Telemetry;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Queues;

/// <summary>
/// Proves the behavior of QueueWriter
/// </summary>
[TestClass]
public class SendPipelineFinalStepFixture
{
    public class MessageWithoutInterface { }

    private SendPipelineFinalStep step = default!;
    private Mock<IBusDataAccess> dataAccess = default!;
    private Mock<IMeters> meters = default!;
    private readonly Mock<ISerializer> _serializer = new();
    private Mock<ISystemClock> clock = default!;

    private QueueData? AddedMessage = null;
    private QueueName? AddedToQueue = default;
    private SendContext context = default!;

    [TestInitialize]
    public void TestInitialize()
    {
        dataAccess = new();
        meters = new();
        _serializer.Reset();
        clock = new();

        clock.SetupGet(c => c.UtcNow).Returns(() => TestData.Now);

        dataAccess.Setup(d => d.AddMessage(It.IsAny<QueueData>(), It.IsAny<QueueName>()))
            .Callback<QueueData, QueueName>((msg, qn) =>
            {
                AddedMessage = msg;
                AddedToQueue = qn;
            })
            .Returns(Task.FromResult<Identity>(new(12345)));

        context = TestData.CreateSendContext();

        _serializer.Setup(x => x.Serialize(context.Message, context.Message.GetType()))
            .Returns(TestData.DefaultBody);

        step = new(
            clock.Object,
            _serializer.Object,
            dataAccess.Object,
            meters.Object);
    }

    /// <summary>
    /// Proves the message cannot be null.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Given_ContextMessageNull_When_Invoke_Then_Throws()
    {
        context.Message = null!;
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            step.Invoke(context, null!));
    }

    /// <summary>
    /// proves the message class is set.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task When_Invoke_Then_HeadersTypeIsSet()
    {
        await step.Invoke(context, null!);
        Assert.AreEqual(new("PeachtreeBus.Abstractions.Tests.TestClasses.TestQueuedMessage, PeachtreeBus.Abstractions.Tests"),
            AddedMessage?.Headers?.MessageClass);
    }

    /// <summary>
    /// Proves the supplied NotBefore is used
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Given_ContextNotBefore_When_Invoke_ContextNotBeforeIsUsed()
    {
        UtcDateTime notBefore = DateTime.UtcNow;
        context.NotBefore = notBefore;
        await step.Invoke(context, null!);

        Assert.IsNotNull(AddedMessage);
        Assert.AreEqual(notBefore, AddedMessage.NotBefore);
    }

    /// <summary>
    /// Proves Enqueued is set to now
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task When_Invoke_SetsEnqueuedToUtcNow()
    {
        await step.Invoke(context, null!);

        Assert.IsNotNull(AddedMessage);
        Assert.AreEqual(TestData.Now, AddedMessage.Enqueued);
    }

    /// <summary>
    /// Proves completed defaults to null
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task When_Invoke_SetsCompletedToNull()
    {
        await step.Invoke(context, null!);

        Assert.IsNotNull(AddedMessage);
        Assert.IsFalse(AddedMessage.Completed.HasValue);
    }

    /// <summary>
    /// Proves failed defaults to null
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task When_Invoke_SetsFailedToNull()
    {
        await step.Invoke(context, null!);

        Assert.IsNotNull(AddedMessage);
        Assert.IsFalse(AddedMessage.Failed.HasValue);
    }

    /// <summary>
    /// Proves retries defaults to zero
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task When_Invoke_SetsRetriesToZero()
    {
        await step.Invoke(context, null!);

        Assert.IsNotNull(AddedMessage);
        Assert.AreEqual(0, AddedMessage.Retries);
    }

    [TestMethod]
    public async Task When_Invoke_Then_MessageIdIsNotEmpty()
    {
        await step.Invoke(context, null!);

        Assert.IsNotNull(AddedMessage);
        Assert.AreNotEqual(UniqueIdentity.Empty, AddedMessage.MessageId);
    }

    /// <summary>
    /// Proves headers are serialized.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task When_Invoke_UserHeadersAreSet()
    {
        await step.Invoke(context, null!);
        Assert.AreSame(context.UserHeaders, AddedMessage?.Headers?.UserHeaders);
    }

    /// <summary>
    /// proves body is serialized.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task When_Invoke_UsesBodyFromSerializer()
    {
        await step.Invoke(context, null!);

        Assert.IsNotNull(AddedMessage);
        Assert.AreEqual(TestData.DefaultBody, AddedMessage.Body);
    }

    /// <summary>
    /// Proves counters are invoked
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task When_Invoke_CountSentMessages()
    {
        await step.Invoke(context, null!);

        meters.Verify(c => c.SentMessage(1), Times.Once);
    }

    /// <summary>
    /// proves Data Access add message is used.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task When_Invoke_InvokesDataAccess()
    {
        await step.Invoke(context, null!);

        dataAccess.Verify(d => d.AddMessage(It.IsAny<QueueData>(), TestData.DefaultQueueName), Times.Once);
    }

    [TestMethod]
    public async Task When_Invoke_Then_Activity()
    {
        using var listener = new TestActivityListener(ActivitySources.Messaging);

        await step.Invoke(context, null);

        var activity = listener.ExpectOneCompleteActivity();
        SendActivityFixture.AssertActivity(activity, context);
    }

    /// <summary>
    /// Proves the correct queue is used.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task WhenInvoke_SendsToCorrectQueue()
    {
        var expected = new QueueName("FooBazQueue");
        context.Destination = expected;
        await step.Invoke(context, null!);
        Assert.AreEqual(expected, AddedToQueue);
    }

    [TestMethod]
    public async Task Given_MessageIsNotIQueuedMessage_When_Invoke_Then_ThrowsUsefulException()
    {
        context.Message = new object();
        await Assert.ThrowsExactlyAsync<TypeIsNotIQueueMessageException>(() =>
            step.Invoke(context, null!));
    }

    [TestMethod]
    public async Task Given_Priority_When_Publish_Then_PriorityIsSet()
    {
        context.MessagePriority = 151;
        await step.Invoke(context, null!);

        Assert.IsNotNull(AddedMessage);
        Assert.AreEqual(151, AddedMessage.Priority);
    }

    [TestMethod]
    public async Task Given_UserHeaders_When_Publish_Then_UserHeadersAreUsed()
    {
        context.Data.Headers.UserHeaders = TestData.DefaultUserHeaders;
        await step.Invoke(context, null!);
        Assert.AreSame(context.UserHeaders, AddedMessage?.Headers?.UserHeaders);
    }
}
