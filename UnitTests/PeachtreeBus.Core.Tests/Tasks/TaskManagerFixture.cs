using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Tasks;

[TestClass]
public class TaskManagerFixture
{
    private TaskManager _manager = default!;
    private readonly Mock<IStarters> _starters = new();
    private readonly Mock<IDelayFactory> _delayFactory = new();
    private CancellationTokenSource _cts = default!;
    private readonly List<Action<Task>> _continues = [];
    private int _runCount = 0;

    [TestInitialize]
    public void Initialize()
    {
        _starters.Reset();

        _cts = new();

        _starters.Setup(s => s.RunStarters(It.IsAny<Action<Task>>(), _cts.Token))
            .Callback((Action<Task> continueWith, CancellationToken token) =>
            {
                _runCount--;
                if (_runCount == 0) _cts.Cancel();
                continueWith(Task.CompletedTask);
            })
            .ReturnsAsync((Action<Task> continuewith, CancellationToken token) =>
            {
                return [Task.Delay(1, CancellationToken.None)
                    .ContinueWith(continuewith, CancellationToken.None)];
            });

        _delayFactory.Setup(x => x.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _continues.Clear();

        _manager = new(
            _delayFactory.Object,
            _starters.Object);
    }

    private void StartersCallback(Action<Task> continueWith, CancellationToken token)
    {
        _runCount--;
        if (_runCount <= 0) _cts.Cancel();
        continueWith(Task.CompletedTask);
    }

    private static List<Task> GetDelayTask(Action<Task> continueWith) =>
    [
        Task.Delay(1, CancellationToken.None).ContinueWith(continueWith, CancellationToken.None),
    ];


    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    public async Task Given_ShouldRunNTimes_When_Run_Then_RunsNTimes(int count)
    {
        _runCount = count;
        var t = Task.Run(() => _manager.Run(_cts.Token));
        await t;
        _starters.Verify(s => s.RunStarters(It.IsAny<Action<Task>>(), _cts.Token), Times.Exactly(count));
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    public async Task Given_StartersReturnsEmpty_When_Run_Then_Delay(int count)
    {
        _runCount = count;

        _starters.Setup(s => s.RunStarters(It.IsAny<Action<Task>>(), _cts.Token))
            .Callback(StartersCallback)
            .ReturnsAsync(() => []);

        var source = new TaskCompletionSource();

        _delayFactory.Setup(x => x.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(source.Task);

        var t = Task.Run(() => _manager.Run(_cts.Token));
        await Task.Delay(10);
        Assert.IsFalse(t.IsCompleted);
        source.SetResult();
        await t;

        _starters.Verify(s => s.RunStarters(It.IsAny<Action<Task>>(), _cts.Token), Times.Exactly(count));
        _delayFactory.Verify(x => x.Delay(TimeSpan.FromSeconds(1), CancellationToken.None), Times.AtLeastOnce());
    }
}
