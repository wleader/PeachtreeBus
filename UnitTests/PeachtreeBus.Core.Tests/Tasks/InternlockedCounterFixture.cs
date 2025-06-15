using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Tasks;

namespace PeachtreeBus.Core.Tests.Tasks;

[TestClass]
public class InternlockedCounterFixture
{
    [TestMethod]
    public void When_New_Then_ValueIsZero()
    {
        var counter = new InterlockedCounter();
        Assert.AreEqual(0, counter.Value);
    }

    [TestMethod]
    public void When_SetValue_Then_ValueIsSet()
    {
        var counter = new InterlockedCounter() { Value = 100 };
        Assert.AreEqual(100, counter.Value);
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(10)]
    public void When_Increment_Then_Value_Increases(int timesToIncrement)
    {
        var counter = new InterlockedCounter { Value = 0 };
        for (int i = 1; i <= timesToIncrement; i++)
        {
            counter.Increment();
            Assert.AreEqual(i, counter.Value);
        }
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(10)]
    public void When_Decrement_Then_Value_Increases(int timesToIncrement)
    {
        var counter = new InterlockedCounter { Value = timesToIncrement + 1 };
        for (int i = timesToIncrement; i > 0; i--)
        {
            counter.Decrement();
            Assert.AreEqual(i, counter.Value);
        }
    }

    [TestMethod]
    public void Given_ValueIsZero_When_Decrement_Then_ValueIsMinusOne()
    {
        var counter = new InterlockedCounter { Value = 0 };
        counter.Decrement();
        Assert.AreEqual(-1, counter.Value);
    }

    [TestMethod]
    public void Given_ValueIsMinusOne_When_Increment_Then_ValueZero()
    {
        var counter = new InterlockedCounter() { Value = -1 };
        counter.Increment();
        Assert.AreEqual(0, counter.Value);
    }

    [TestMethod]
    public void Given_ValueIsMaxInt_When_Increment_Then_Rollover()
    {
        var counter = new InterlockedCounter() { Value = int.MaxValue };
        counter.Increment();
        Assert.AreEqual(int.MinValue, counter.Value);
    }


    [TestMethod]
    public void Given_ValueIsMinInt_When_Deccrement_Then_Rollover()
    {
        var counter = new InterlockedCounter() { Value = int.MinValue };
        counter.Decrement();
        Assert.AreEqual(int.MaxValue, counter.Value);
    }
}