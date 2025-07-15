using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Tasks;

[TestClass]
public class TaskManagerFixture
{
    private TaskManager _manager = default!;
    private readonly Mock<ICurrentTasks> _tasks = new();
    private readonly Mock<IStarters> _starters = new();
    private readonly Mock<IDelayFactory> _delayFactory = new();
    private CancellationTokenSource _cts = default!;
    private int _runCount = 0;
    private readonly Task _delayTask = Task.Delay(-1);

    [TestInitialize]
    public void Initialize()
    {
        _tasks.Reset();
        _starters.Reset();
        _delayFactory.Reset();

        _cts = new();

        _starters.Setup(s => s.RunStarters(_cts.Token))
            .Callback((CancellationToken token) =>
            {
                _runCount--;
                if (_runCount <= 0) _cts.Cancel();
            });

        _delayFactory.Setup(x => x.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(() => _delayTask);

        _tasks.Setup(t => t.WhenAll()).Returns(Task.CompletedTask);
        _tasks.Setup(t => t.WhenAny()).Returns(Task.CompletedTask);

        _manager = new(
            _tasks.Object,
            _delayFactory.Object,
            _starters.Object);
    }

    [TestMethod]
    [DataRow(1, 0, 1, DisplayName = "RunOnceExpectDelay")]
    [DataRow(1, 1, 0, DisplayName = "RunOnceNoDelay")]
    [DataRow(2, 2, 0, DisplayName = "RunTwiceNoDelay")]
    [DataRow(2, 0, 2, DisplayName = "RunTwiceExpectDelay")]
    public async Task Given_RunCount_And_TaskCount_When_Run_Then_StartCount_And_DelayCount(int count, int taskCount, int expectedDelays)
    {
        _runCount = count;

        _tasks.SetupGet(t => t.Count).Returns(taskCount);

        var t = Task.Run(() => _manager.Run(_cts.Token));
        await t;

        _starters.Verify(s => s.RunStarters(_cts.Token), Times.Exactly(count));
        _tasks.Verify(t => t.WhenAny(), Times.Exactly(count));
        _tasks.Verify(t => t.WhenAll(), Times.Once);

        _delayFactory.Verify(x => x.Delay(
            TimeSpan.FromSeconds(1),
            CancellationToken.None),
            Times.Exactly(expectedDelays));

        _tasks.Verify(t => t.Add(_delayTask), Times.Exactly(expectedDelays));
    }
}
