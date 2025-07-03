using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Core.Tests.Telemetry;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using PeachtreeBus.Telemetry;
using PeachtreeBus.Testing;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Queues;

[TestClass]
public class ProcessQueuedTaskFixture
{
    private ProcessQueuedTask _task = default!;
    private readonly Mock<IBusConfiguration> _busConfiguration = new();
    private readonly Mock<ILogger<ProcessQueuedTask>> _log = new();
    private readonly Mock<IQueueReader> _reader = new();
    private readonly Mock<IMeters> _meters = new();
    private readonly Mock<IBusDataAccess> _dataAccess = new();
    private readonly Mock<IQueuePipelineInvoker> _invoker = new();
    private readonly FakeClock _clock = new();

    private QueueContext? _context;

    private bool _savepointCreated;
    private bool _pipelineInvoked;
    private bool _messageCompleted;
    private bool _messageFailed;
    private bool _messageDelayed;
    private bool _rolledBack;

    [TestInitialize]
    public void Initialize()
    {
        _busConfiguration.Reset();
        _log.Reset();
        _reader.Reset();
        _meters.Reset();
        _dataAccess.Reset();
        _invoker.Reset();
        _clock.Reset();

        _context = TestData.CreateQueueContext();

        _reader.Setup(r => r.GetNext(TestData.DefaultQueueName))
            .ReturnsAsync(() => _context);

        _busConfiguration.SetupGet(c => c.QueueConfiguration)
            .Returns(new QueueConfiguration()
            {
                QueueName = TestData.DefaultQueueName
            });

        _savepointCreated = false;
        _pipelineInvoked = false;
        _messageCompleted = false;
        _messageFailed = false;
        _messageDelayed = false;
        _rolledBack = false;

        _dataAccess.Setup(d => d.CreateSavepoint("BeforeMessageHandler"))
            .Callback(() => _savepointCreated = true);
        _dataAccess.Setup(d => d.RollbackToSavepoint("BeforeMessageHandler"))
            .Callback(() =>
            {
                Assert.IsTrue(_savepointCreated, "Rollback to savepoint before creating save point");
                _rolledBack = true;
            });

        _invoker.Setup(i => i.Invoke(It.IsAny<QueueContext>()))
            .Callback((QueueContext c) =>
            {
                Assert.AreSame(_context, c);
                Assert.IsTrue(_savepointCreated, "Pipeline Invoked without creating save point.");
                _pipelineInvoked = true;
            });
        _reader.Setup(r => r.Complete(It.IsAny<QueueContext>()))
            .Callback(() =>
            {
                Assert.IsTrue(_pipelineInvoked, "Message compeleted without invoking pipeline.");
                _messageCompleted = true;
                Assert.IsFalse(_messageFailed, "Complete Fail and Delay are exlusive.");
                Assert.IsFalse(_messageDelayed, "Complete Fail and Delay are exlusive.");
            });
        _reader.Setup(r => r.Fail(It.IsAny<QueueContext>(), It.IsAny<Exception>()))
            .Callback(() =>
            {
                Assert.IsTrue(_pipelineInvoked, "Message compeleted without invoking pipeline.");
                _messageFailed = true;
                Assert.IsFalse(_messageCompleted, "Complete Fail and Delay are exlusive.");
                Assert.IsFalse(_messageDelayed, "Complete Fail and Delay are exlusive.");
            });
        _reader.Setup(r => r.DelayMessage(It.IsAny<QueueContext>(), It.IsAny<int>()))
            .Callback(() =>
            {
                Assert.IsTrue(_pipelineInvoked, "Message Delayed without invoking pipeline.");
                Assert.IsTrue(_rolledBack, "Delay must happen after savepoint rollback.");
                _messageDelayed = true;
                Assert.IsFalse(_messageCompleted, "Complete Fail and Delay are exlusive.");
                Assert.IsFalse(_messageFailed, "Complete Fail and Delay are exlusive.");
            });

        _task = new(
            _busConfiguration.Object,
            _clock,
            _log.Object,
            _reader.Object,
            _meters.Object,
            _dataAccess.Object,
            _invoker.Object);
    }

    [TestMethod]
    public async Task Given_NoPendingMessages_When_DoWork_Then_False()
    {
        _reader.Setup(r => r.GetNext(TestData.DefaultQueueName))
            .ReturnsAsync((QueueContext?)null);
        Assert.IsFalse(await _task.RunOne());
        _dataAccess.VerifyNoOtherCalls();
        _invoker.Verify(i => i.Invoke(_context!), Times.Never);
        _reader.Verify(r => r.Complete(_context!), Times.Never);
        _meters.Verify(m => m.FinishMessage(), Times.Never);
        _meters.Verify(m => m.StartMessage(), Times.Never);
    }

    [TestMethod]
    public async Task Given_ReaderReturns_When_DoWork_Then_SavepointCreated_And_PipelineInvoked_And_MessageCompleted()
    {
        Assert.IsTrue(await _task.RunOne());
        _reader.Verify(r => r.GetNext(TestData.DefaultQueueName), Times.Once());
        _dataAccess.Verify(d => d.CreateSavepoint("BeforeMessageHandler"), Times.Once);
        _dataAccess.VerifyNoOtherCalls();
        _invoker.Verify(i => i.Invoke(_context!), Times.Once());
        _reader.Verify(r => r.Complete(_context!), Times.Once());
        _meters.Verify(m => m.FinishMessage(), Times.Once());
        _meters.Verify(m => m.StartMessage(), Times.Once());
    }

    [TestMethod]
    public async Task Given_PipelineThrows_When_DoWork_Then_()
    {
        var ex = new TestException();
        _invoker.Setup(i => i.Invoke(_context!))
            .Callback(() => _pipelineInvoked = true)
            .ThrowsAsync(ex);

        Assert.IsTrue(await _task.RunOne());

        _reader.Verify(r => r.GetNext(TestData.DefaultQueueName), Times.Once());
        _dataAccess.Verify(d => d.CreateSavepoint("BeforeMessageHandler"), Times.Once);
        _invoker.Verify(i => i.Invoke(_context!), Times.Once);
        _dataAccess.Verify(d => d.RollbackToSavepoint("BeforeMessageHandler"), Times.Once);
        _reader.Verify(r => r.Fail(_context!, ex), Times.Once);
        _meters.Verify(m => m.FinishMessage(), Times.Once());
        _meters.Verify(m => m.StartMessage(), Times.Once());
        _dataAccess.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task Given_PipelineThrows_When_DoWork_Then_ActivityHasError()
    {
        var ex = new TestException();
        _invoker.Setup(i => i.Invoke(_context!))
            .Callback(() => _pipelineInvoked = true)
            .ThrowsAsync(ex);

        using var listener = new TestActivityListener(ActivitySources.Messaging);
        _ = await _task.RunOne();

        var activity = listener.ExpectOneCompleteActivity();
        activity.AssertException(ex);
    }

    [TestMethod]
    public async Task Given_AMessage_When_DoWork_Then_Activity()
    {
        using var listener = new TestActivityListener(ActivitySources.Messaging);

        _ = await _task.RunOne();

        var activity = listener.ExpectOneCompleteActivity();
        ReceiveActivityFixture.AssertActivity(activity, _context!, _clock.UtcNow);
    }

    [TestMethod]
    public async Task Given_NoMessage_When_DoWork_Then_Activity()
    {
        using var listener = new TestActivityListener(ActivitySources.Messaging);
        _context = null;
        _ = await _task.RunOne();
        listener.AssertNoActivity();
    }

    [TestMethod]
    public async Task Given_SagaIsBlocked_When_DoWork_Then_MessageIsDelayed()
    {
        _context!.SagaData = new()
        {
            Blocked = true,
            Data = new("Data"),
            Key = new("SagaKey"),
            MetaData = TestData.CreateSagaMetaData(),
            SagaId = UniqueIdentity.New(),
        };

        Assert.IsTrue(await _task.RunOne());
        _dataAccess.Verify(d => d.CreateSavepoint("BeforeMessageHandler"), Times.Once);
        _meters.Verify(c => c.SagaBlocked(), Times.Once);
        _dataAccess.Verify(d => d.RollbackToSavepoint("BeforeMessageHandler"), Times.Once);
        _reader.Verify(r => r.DelayMessage(_context, 250), Times.Once);
        _dataAccess.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task Given_QueueConfigurationIsNull_When_DoWork_Then_NoWork()
    {
        _busConfiguration.SetupGet(c => c.QueueConfiguration).Returns((QueueConfiguration)null!);
        Assert.IsFalse(await _task.RunOne());
        _reader.Verify(r => r.GetNext(It.IsAny<QueueName>()), Times.Never);
    }
}
