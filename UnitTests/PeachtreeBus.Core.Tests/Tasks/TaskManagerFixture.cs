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

        _continues.Clear();

        _manager = new(
            _starters.Object);
    }

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
}
