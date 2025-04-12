using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Core.Tests.Telemetry;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Telemetry;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Subscriptions;

[TestClass]
public class ProcessSubscribedTaskFixture
{
    private ProcessSubscribedTask _task = default!;
    private readonly Mock<IBusDataAccess> _dataAccess = new();
    private readonly Mock<IBusConfiguration> _busConfiguration = new();
    private readonly Mock<IMeters> _meters = new();
    private readonly Mock<ISubscribedPipelineInvoker> _invoker = new();
    private readonly Mock<ISubscribedReader> _reader = new();
    private readonly FakeClock _clock = new();

    private SubscribedContext? _context;

    private bool _savepointCreated = false;
    private bool _pipelineInvoked = false;

    [TestInitialize]
    public void Initialize()
    {
        _reader.Reset();
        _invoker.Reset();
        _meters.Reset();
        _busConfiguration.Reset();
        _dataAccess.Reset();
        _clock.Reset();

        _savepointCreated = false;
        _pipelineInvoked = false;

        _invoker.Setup(i => i.Invoke(_context!))
            .Callback(() =>
            {
                Assert.IsTrue(_savepointCreated, "Savepoint was not created before invoking the pipeline.");
                _pipelineInvoked = true;
            });
        _busConfiguration.Given_SubscriptionConfiguration();
        _dataAccess.DisallowTransactions();
        _context = TestData.CreateSubscribedContext();
        _reader.Setup(r => r.GetNext(TestData.DefaultSubscriberId))
            .ReturnsAsync(() => _context);
        _reader.Setup(r => r.Complete(_context))
            .Callback(() =>
            {
                Assert.IsTrue(_pipelineInvoked, "Message completed without invoking pipeline.");
            });

        _task = new(
            FakeLog.Create<ProcessSubscribedTask>(),
            _clock,
            _reader.Object,
            _busConfiguration.Object,
            _meters.Object,
            _dataAccess.Object,
            _invoker.Object);
    }


    [TestCleanup]
    public void TestCleanup()
    {
        _dataAccess.Verify();
    }

    [TestMethod]
    public async Task Given_NoPendingMessages_When_DoWork_Then_ReturnFalse()
    {
        _reader.Setup(r => r.GetNext(It.IsAny<SubscriberId>()))
            .ReturnsAsync((SubscribedContext)null!);
        Assert.IsFalse(await _task.RunOne());
    }

    [TestMethod]
    public async Task Given_AMessage_When_DoWork_Then_IncrementCounters()
    {
        Assert.IsTrue(await _task.RunOne());
        _meters.Verify(c => c.StartMessage(), Times.Once);
        _meters.Verify(c => c.FinishMessage(), Times.Once);
    }

    [TestMethod]
    public async Task Given_AMessage_When_DoWork_Then_Activity()
    {
        using var listener = new TestActivityListener(ActivitySources.Messaging);
        Assert.IsTrue(await _task.RunOne());
        var activity = listener.ExpectOneCompleteActivity();
        ReceiveActivityFixture.AssertActivity(activity, _context!, _clock.UtcNow);
    }

    [TestMethod]
    public async Task Given_AMessage_When_DoWork_Then_CreatesSavepoint_And_InvokesPipeline_And_Completes()
    {
        Assert.IsTrue(await _task.RunOne());
        _reader.Verify(r => r.GetNext(TestData.DefaultSubscriberId), Times.Once);
        _dataAccess.Verify(d => d.CreateSavepoint("BeforeSubscriptionHandler"), Times.Once);
        _invoker.Verify(d => d.Invoke(_context!), Times.Once);
        _reader.Verify(r => r.Complete(_context!), Times.Once);
    }

    [TestMethod]
    public async Task Given_PipelineThrows_When_DoWork_Then_RollsBackToSavepoint_And_ActivityHasException_And_MessageFails()
    {
        var ex = new TestException();
        _invoker.Setup(i => i.Invoke(_context!))
            .ThrowsAsync(ex);
        using var listener = new TestActivityListener(ActivitySources.Messaging);

        Assert.IsTrue(await _task.RunOne());
        _reader.Verify(r => r.GetNext(TestData.DefaultSubscriberId), Times.Once);
        _dataAccess.Verify(d => d.CreateSavepoint("BeforeSubscriptionHandler"), Times.Once);
        _invoker.Verify(d => d.Invoke(_context!), Times.Once);
        _dataAccess.Verify(d => d.RollbackToSavepoint("BeforeSubscriptionHandler"), Times.Once);
        _reader.Verify(r => r.Fail(_context!, ex), Times.Once);

        var activity = listener.ExpectOneCompleteActivity();
        activity.AssertException(ex);
    }
}
