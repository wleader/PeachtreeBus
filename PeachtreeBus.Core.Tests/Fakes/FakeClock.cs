using System;

namespace PeachtreeBus.Core.Tests.Fakes;

public class FakeClock : ISystemClock
{
    public DateTime UtcNow => GetNow();

    public Func<DateTime> GetNow = () => TestData.Now;

    public void Reset()
    {
        GetNow = () => TestData.Now;
    }
}
