using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Tasks;

namespace PeachtreeBus.Core.Tests.Fakes;

public class FakeTaskCounter : IMessagingTaskCounter, IScheduledTaskCounter
{
    private int _value;
    private int _available;
    private int? _expectedTaskCount;
    private int _totalStarted;
    private int _totalFinished;
    private Exception? _availableException;

    public void Reset()
    {
        Interlocked.Exchange(ref _value, 0);
        Interlocked.Exchange(ref _totalStarted, 0);
        Interlocked.Exchange(ref _totalFinished, 0);
        _expectedTaskCount = null;
        _availableException = null;
    }
    
    public Action? AvailableCallback { get; set; }

    public void Given_ExpectedTaskCount(int expectedTaskCount) => 
        _expectedTaskCount = expectedTaskCount;

    public int Increment()
    {
        var result = Interlocked.Increment(ref _value);
        var t = Interlocked.Increment(ref _totalStarted);
        Assert.IsFalse(_expectedTaskCount.HasValue && t > _expectedTaskCount.Value,
            $"{t} Tasks were started when only {_expectedTaskCount} tasks were expected.");
        return result;
    }

    public int Decrement()
    {
        var result = Interlocked.Decrement(ref _value);
        var t = Interlocked.Increment(ref _totalFinished);
        Assert.IsFalse(_expectedTaskCount.HasValue && t > _expectedTaskCount.Value,
            $"{t} Tasks were finished when only {_expectedTaskCount} tasks were expected.");
        return result;
    }

    public int Value => Interlocked.CompareExchange(ref _value, 0, 0);

    public int Available()
    {
        AvailableCallback?.Invoke();
        return _availableException is null
            ? _available
            : throw _availableException;
    }

    public void Given_Available(int value) => _available = value;
    public void Given_AvailableThrows(Exception exception) => _availableException = exception;

    public void Then_TasksComplete()
    {
        SpinWait.SpinUntil(() => Value == 0, TimeSpan.FromSeconds(1));
        if (_expectedTaskCount.HasValue)
        {
            var totalStarted = Interlocked.CompareExchange(ref _totalStarted, 0, 0);
            Assert.AreEqual(_expectedTaskCount.Value, totalStarted,
                $"The expected number of tasks did not start.");

            var totalFinished = Interlocked.CompareExchange(ref _totalFinished, 0, 0);
            Assert.AreEqual(_expectedTaskCount.Value, totalFinished,
            $"The expected number of tasks did not complete.");
        }
    }
}