using Moq;
using PeachtreeBus.Tasks;

namespace PeachtreeBus.Core.Tests.Fakes;

public static class MockTrackerExtensions
{
    public static Mock<T> Given_Due<T>(this Mock<T> tracker, bool result = true)
        where T : class, ITracker
    {
        tracker.SetupGet(t => t.ShouldStart).Returns(result);
        return tracker;
    }
}
