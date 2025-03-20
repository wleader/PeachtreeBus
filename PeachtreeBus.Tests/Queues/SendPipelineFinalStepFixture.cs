using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Exceptions;
using PeachtreeBus.Queues;
using PeachtreeBus.Tests.Fakes;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Queues;

/// <summary>
/// Proves the behavior of QueueWriter
/// </summary>
[TestClass]
public class SendPipelineFinalStepFixture
{
    public class MessageWithoutInterface { }

    private SendPipelineFinalStep step = default!;
    private Mock<IBusDataAccess> dataAccess = default!;
    private Mock<IPerfCounters> counters = default!;
    private FakeSerializer serializer = default!;
    private Mock<ISystemClock> clock = default!;

    private QueueData? AddedMessage = null;
    private QueueName? AddedToQueue = default;
    private SendContext context = default!;

    [TestInitialize]
    public void TestInitialize()
    {
        dataAccess = new();
        counters = new();
        serializer = new();
        clock = new();

        clock.SetupGet(c => c.UtcNow).Returns(() => TestData.Now);

        dataAccess.Setup(d => d.AddMessage(It.IsAny<QueueData>(), It.IsAny<QueueName>()))
            .Callback<QueueData, QueueName>((msg, qn) =>
            {
                AddedMessage = msg;
                AddedToQueue = qn;
            })
            .Returns(Task.FromResult<Identity>(new(12345)));

        context = new()
        {
            Destination = TestData.DefaultQueueName,
            Message = TestData.CreateQueueUserMessage(),
            Headers = TestData.DefaultUserHeaders,
        };

        step = new(clock.Object, serializer.Object, dataAccess.Object, counters.Object);
    }

    /// <summary>
    /// Proves the message cannot be null.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Given_ContextMessageNull_When_Invoke_Then_Throws()
    {
        context.Message = null!;
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
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
        Assert.AreEqual(1, serializer.SerializedHeaders.Count);
        Assert.AreEqual("PeachtreeBus.Tests.TestData+TestQueuedMessage, PeachtreeBus.Tests",
            serializer.SerializedHeaders[0].MessageClass);
    }

    /// <summary>
    /// Proves that NotBefore defaults to Now
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Given_ContextNotBeforeNull_When_Invoke_NotBeforeIsDefaultedToNow()
    {
        context.NotBefore = null;
        await step.Invoke(context, null!);

        Assert.IsNotNull(AddedMessage);
        Assert.AreEqual(TestData.Now, AddedMessage.NotBefore);
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

    /// <summary>
    /// Proves headers are serialized.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task When_Invoke_UsesHeadersFromSerializer()
    {
        await step.Invoke(context, null!);

        Assert.IsNotNull(AddedMessage);
        Assert.AreEqual(serializer.SerializeHeadersResult, AddedMessage.Headers);
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
        Assert.AreEqual(serializer.SerializeMessageResult, AddedMessage.Body);
    }

    /// <summary>
    /// Proves counters are invoked
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task When_Invoke_CountSentMessages()
    {
        await step.Invoke(context, null!);

        counters.Verify(c => c.SentMessage(), Times.Once);
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
        await Assert.ThrowsExceptionAsync<TypeIsNotIQueueMessageException>(() =>
            step.Invoke(context, null!));
    }

    [TestMethod]
    public async Task Given_Priority_When_Publish_Then_PriorityIsSet()
    {
        context.Priority = 151;
        await step.Invoke(context, null!);

        Assert.IsNotNull(AddedMessage);
        Assert.AreEqual(151, AddedMessage.Priority);
    }

    [TestMethod]
    public async Task Given_UserHeaders_When_Publish_Then_UserHeadersAreUsed()
    {
        context.Headers = TestData.DefaultUserHeaders;
        await step.Invoke(context, null!);

        Assert.AreEqual(1, serializer.SerializedHeaders.Count);
        Assert.AreSame(TestData.DefaultUserHeaders, serializer.SerializedHeaders[0].UserHeaders);
    }
}
