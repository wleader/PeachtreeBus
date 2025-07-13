using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Tasks;

namespace PeachtreeBus.Core.Tests.Tasks;

[TestClass]
public class ScheduledTaskCounterFixture
{
    private ScheduledTaskCounter _counter = default!;

    [TestInitialize]
    public void Initialize()
    {
        _counter = new();
    }

    [TestMethod]
    public void Given_ValueIsZero_When_Increment_Then_ValueIsUnchanged()
    {
        Assert.AreEqual(0, _counter.Value);
        _counter.Increment();
        Assert.AreEqual(0, _counter.Value);
    }

    [TestMethod]
    public void Given_ValueIsZero_When_Decrement_Then_ValueIsUnchanged()
    {
        Assert.AreEqual(0, _counter.Value);
        _counter.Decrement();
        Assert.AreEqual(0, _counter.Value);
    }

    [TestMethod]
    public void Given_AvailableIsOne_When_Increment_Then_AvailableIsUnchanged()
    {
        Assert.AreEqual(1, _counter.Available());
        _counter.Increment();
        Assert.AreEqual(1, _counter.Available());
    }

    [TestMethod]
    public void Given_AvailableIsOne_When_Decrement_Then_AvailableIsUnchanged()
    {
        Assert.AreEqual(1, _counter.Available());
        _counter.Decrement();
        Assert.AreEqual(1, _counter.Available());
    }
}
