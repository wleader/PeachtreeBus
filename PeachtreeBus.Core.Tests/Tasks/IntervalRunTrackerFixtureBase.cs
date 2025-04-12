using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Tasks;
using System;

namespace PeachtreeBus.Core.Tests.Tasks;

public abstract class IntervalRunTrackerFixtureBase<TTracker>
    where TTracker : IntervalRunTracker
{
    protected TTracker _tracker = default!;
    protected FakeClock _clock = new();
    protected Mock<IBusConfiguration> _busConfiguration = new();

    [TestInitialize]
    public void Initialize()
    {
        _clock.Reset();
        _busConfiguration.Reset();

        _busConfiguration.Given_SubscriptionConfiguration();

        _tracker = CreateTracker();
    }

    protected abstract TTracker CreateTracker();
    protected abstract void Given_Configuration();
    protected abstract void Given_NoConfiguration();

    [TestMethod]
    public void Given_Configuration_And_NewTracker_Then_Due()
    {
        Given_Configuration();
        var t = CreateTracker();
        Assert.AreEqual(DateTime.MinValue, t.NextDue);
        Assert.IsTrue(t.Interval.HasValue);
        Assert.IsTrue(t.ShouldStart);
    }

    [TestMethod]
    public void Given_Configuration_And_WorkDone_Then_ShouldNotStart()
    {
        Given_Configuration();
        _tracker.WorkDone();
        Assert.IsFalse(_tracker.ShouldStart);
    }

    [TestMethod]
    public void Given_Configuration_And_WorkDone_When_Elapsed_Then_ShouldStart()
    {
        Given_Configuration();
        _tracker.WorkDone();
        _clock.GetNow = () => _tracker.NextDue.AddMilliseconds(1);
        Assert.IsTrue(_tracker.ShouldStart);
    }

    [TestMethod]
    public void Given_NoConfiguration_And_NewTracker_Then_ShouldNotStart()
    {
        Given_NoConfiguration();
        var t = CreateTracker();
        Assert.IsFalse(t.ShouldStart);
    }

    [TestMethod]
    public void Given_NoConfiguration_And_WorkDone_Then_ShouldNotStart()
    {
        Given_NoConfiguration();
        _tracker.WorkDone();
        Assert.IsFalse(_tracker.ShouldStart);
    }

    [TestMethod]
    public void Given_NoConfiguration_And_WorkDone_When_Elapsed_Then_ShouldNotStart()
    {
        Given_NoConfiguration();
        _tracker.WorkDone();
        _clock.GetNow = () => DateTime.MaxValue;
        Assert.IsFalse(_tracker.ShouldStart);
    }
}
