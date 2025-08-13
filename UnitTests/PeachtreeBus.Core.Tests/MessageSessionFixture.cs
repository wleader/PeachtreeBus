using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Abstractions.Tests.TestClasses;
using PeachtreeBus.Exceptions;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests;

[TestClass]
public class MessageSessionFixture
{
    private MessagingSession _session = default!;
    private readonly Mock<IQueueWriter> _queueWriter = new();
    private readonly Mock<ISubscribedPublisher> _publisher = new();
    private readonly Mock<IBusConfiguration> _busConfiguration = new();
    private QueueConfiguration _queueConfiguration = default!;
    private readonly QueueName _queueName = new("OtherQueue");
    private readonly DateTime _notBefore = TestData.Now;
    private const int _priority = 10;
    private readonly UserHeaders _userHeader = [];
    private const bool _newConversation = true;
    private readonly TestQueuedMessage _queueMessage = new();
    private readonly TestSubscribedMessage _subscribedMessage = new();
    private readonly Topic _topic = new("SomeTopic");

    [TestInitialize]
    public void Initialize()
    {
        _queueWriter.Reset();
        _publisher.Reset();
        _busConfiguration.Reset();

        _queueConfiguration = new()
        {
            QueueName = new("QueueName")
        };

        _busConfiguration.SetupGet(x => x.QueueConfiguration)
            .Returns(() => _queueConfiguration);

        _session = new(
            _queueWriter.Object,
            _publisher.Object,
            _busConfiguration.Object);
    }

    [TestMethod]
    public async Task Given_MessageSession_When_SendBasic_Then_QueueWriterCalled()
    {
        await _session.Send(_queueMessage, _queueName);
        _queueWriter.Verify(x => x.WriteMessage(
            _queueName, _queueMessage, null, 0, null, false),
            Times.Once);
        _queueWriter.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task Given_MessageSession_When_SendWithDetails_Then_QueueWriterCalled()
    {
        await _session.Send(_queueMessage, _queueName,
            _notBefore, _priority, _userHeader, _newConversation);
        _queueWriter.Verify(x => x.WriteMessage(
            _queueName, _queueMessage, _notBefore, _priority, _userHeader, _newConversation),
            Times.Once);
        _queueWriter.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task Given_MessageSession_When_SendLocal_Then_QueueWriterCalled()
    {
        await _session.SendLocal(_queueMessage);
        _queueWriter.Verify(x => x.WriteMessage(
            _queueConfiguration.QueueName, _queueMessage, null, 0, null, false),
            Times.Once);
        _queueWriter.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task Given_NoQueueConfiguration_When_SendLocal_Then_Throws()
    {
        _queueConfiguration = null!;
        await Assert.ThrowsExactlyAsync<ConfigurationException>(() =>
            _session.SendLocal(_queueMessage));
    }

    [TestMethod]
    public async Task Given_MessageSession_When_PublishBasic_Then_PublisherCalled()
    {
        await _session.Publish(_topic, _subscribedMessage);
        _publisher.Verify(x => x.Publish(
            _topic, _subscribedMessage, null, 0, null, false));
        _publisher.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task Given_MessageSession_When_Publish_Then_PublisherCalled()
    {
        await _session.Publish(_topic, _subscribedMessage, _notBefore, _priority, _userHeader, _newConversation);
        _publisher.Verify(x => x.Publish(
            _topic, _subscribedMessage, _notBefore, _priority, _userHeader, _newConversation));
        _publisher.VerifyNoOtherCalls();
    }
}
