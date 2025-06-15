using Moq;
using System;

namespace PeachtreeBus.Core.Tests.Fakes;

public class FakeClock : ISystemClock
{
    private readonly Mock<ISystemClock> _clock;

    public DateTime UtcNow => _clock.Object.UtcNow;

    public FakeClock()
    {
        _clock = new();
        Reset();
    }

    public void Reset()
    {
        Returns(TestData.Now);
    }

    public void Returns(DateTime value)
    {
        _clock.SetupGet(x => x.UtcNow).Returns(value);
    }
}
