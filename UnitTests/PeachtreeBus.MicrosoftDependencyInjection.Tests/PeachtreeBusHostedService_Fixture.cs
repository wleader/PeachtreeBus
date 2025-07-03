using Moq;
using PeachtreeBus.Tasks;
using PeachtreeBus.Testing;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.MicrosoftDependencyInjection.Tests;

[TestClass]
public class PeachtreeBusHostedService_Fixture
{
    private PeachtreeBusHostedService _service = default!;
    private readonly Mock<IScopeFactory> _scopeFactory = new();
    private readonly FakeServiceProviderAccessor _accessor = new(new());
    private readonly Mock<IRunStartupTasks> _runStartupTasks = new();
    private readonly Mock<ITaskManager> _taskManager = new();
    private bool _StartupHasRun = false;
    private TaskCompletionSource _taskCompletionSource = default!;

    [TestInitialize]
    public void Initialize()
    {
        _scopeFactory.Reset();
        _accessor.Reset();
        _runStartupTasks.Reset();
        _taskManager.Reset();
        _taskCompletionSource = new();

        _runStartupTasks.Setup(r => r.RunStartupTasks())
            .Callback(() => { _StartupHasRun = true; });

        _accessor.AddMock(_runStartupTasks);

        _accessor.Add(_taskManager.Object, () => Assert.IsTrue(_StartupHasRun));

        _scopeFactory.Setup(s => s.Create()).Returns(() => _accessor.Object);

        _taskManager.Setup(t => t.Run(It.IsAny<CancellationToken>()))
            .Callback((CancellationToken ct) => ct.Register(_taskCompletionSource.SetResult))
            .Returns(_taskCompletionSource.Task);

        _service = new(_scopeFactory.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _service.Dispose();
    }

    [TestMethod]
    public async Task When_StartAsync_Then_Starts()
    {
        var cts = new CancellationTokenSource();
        await _service.StartAsync(cts.Token);
        _runStartupTasks.Verify(r => r.RunStartupTasks(), Times.Once);
        _runStartupTasks.VerifyNoOtherCalls();
        _taskManager.Verify(t => t.Run(It.IsAny<CancellationToken>()), Times.Once);
        _taskManager.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task When_StopAsync_Then_Stops()
    {
        var cts = new CancellationTokenSource();
        await _service.StartAsync(cts.Token);
        await _service.StopAsync(cts.Token);

        Assert.IsTrue(_taskCompletionSource.Task.IsCompleted);
    }

    [TestMethod]
    public async Task When_StartTokenCancelled_Then_ManagerTokenIsCancelled()
    {
        var cts = new CancellationTokenSource();
        _scopeFactory.Setup(f => f.Create())
            .Callback(cts.Cancel)
            .Returns(_accessor.Object);

        _taskManager.Setup(t => t.Run(It.IsAny<CancellationToken>()))
            .Callback((CancellationToken ct) =>
            {
                ct.Register(_taskCompletionSource.SetResult);
                Assert.IsTrue(ct.IsCancellationRequested);
            })
            .Returns(_taskCompletionSource.Task);

        await _service.StartAsync(cts.Token);
        _taskManager.Verify(t => t.Run(It.IsAny<CancellationToken>()), Times.Once);
        _taskManager.VerifyNoOtherCalls();
    }
}
