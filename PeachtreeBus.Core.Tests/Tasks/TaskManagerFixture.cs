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
    private readonly Mock<ITaskCounter> _counter = new();
    private readonly Mock<IStarters> _starters = new();
    private readonly Mock<ISleeper> _sleeper = new();
    private CancellationTokenSource _cts = default!;
    private readonly List<Action<Task>> _continues = [];
    private readonly Queue<int> _availableResults = new();

    [TestInitialize]
    public void Initialize()
    {
        _counter.Reset();
        _starters.Reset();

        _cts = new();

        _counter.Setup(c => c.Available())
            .Returns(() =>
            {
                if (_availableResults.Count == 1)
                    _cts.Cancel();
                return _availableResults.TryDequeue(out var result)
                    ? result
                    : 0;
            });

        _sleeper.Setup(s => s.Sleep(It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        _starters.Setup(s => s.RunStarters(It.IsAny<Action<Task>>(), _cts.Token))
            .Callback((Action<Task> continueWith, CancellationToken token) =>
            {
                continueWith(Task.CompletedTask);
            });

        _continues.Clear();

        _manager = new(
            _counter.Object,
            _starters.Object,
            _sleeper.Object);
    }

    [TestMethod]
    public async Task Given_ShouldRunOnce_When_Run_Then_RunsOnce()
    {
        _availableResults.Clear();
        _availableResults.Enqueue(
            [0, // first wait for available, so causes a sleep
            1, // second wait for available, breaks the wait loop.
            1, // causes the second sleep.
            // token is canceled, so while loop breaks.
            ]);

        var t = Task.Run(() => _manager.Run(_cts.Token));
        //_cts.Cancel();
        await t;
        _starters.Verify(s => s.RunStarters(It.IsAny<Action<Task>>(), _cts.Token), Times.Once);
        _sleeper.Verify(s => s.Sleep(It.IsAny<int>()), Times.Exactly(2));
        _sleeper.Verify(s => s.Wake(), Times.Exactly(1));
    }


    [TestMethod]
    public async Task Given_ShouldRunTwice_When_Run_Then_RunsTwice()
    {
        _availableResults.Clear();
        _availableResults.Enqueue(
            [0, // first wait for available, so causes a sleep
            1, // second wait for available, breaks the wait loop.
            1, // causes the second sleep.
            1, // wait for available, does not sleep.
            0, // nothing availabl, skipp second sleep.
            // token is canceled, so while loop breaks.
            ]);

        var t = Task.Run(() => _manager.Run(_cts.Token));
        //_cts.Cancel();
        await t;
        _starters.Verify(s => s.RunStarters(It.IsAny<Action<Task>>(), _cts.Token), Times.Exactly(2));
        _sleeper.Verify(s => s.Sleep(It.IsAny<int>()), Times.Exactly(2));
        _sleeper.Verify(s => s.Wake(), Times.Exactly(2));
    }
}
