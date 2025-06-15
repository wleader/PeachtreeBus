using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Tasks;

namespace PeachtreeBus.Core.Tests.Tasks;

[TestClass]
public class AlwaysRunTrackerFixture
{
    private AlwaysRunTracker _tracker = default!;

    [TestInitialize]
    public void Initialize()
    {
        _tracker = new();
    }

    [TestMethod]
    public void Given_Tracker_Then_ShouldStart()
    {
        Assert.IsTrue(_tracker.ShouldStart);
    }


    [TestMethod]
    public void Given_Tracker_And_WorkDone_Then_ShouldStart()
    {
        _tracker.WorkDone();
        Assert.IsTrue(_tracker.ShouldStart);
    }

    [TestMethod]
    public void Given_Tracker_And_Started_Then_ShouldStart()
    {
        _tracker.Start();
        Assert.IsTrue(_tracker.ShouldStart);
    }
}
