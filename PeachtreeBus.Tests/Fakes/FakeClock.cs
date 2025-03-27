using System;

namespace PeachtreeBus.Tests.Fakes;

public class FakeClock : ISystemClock
{
    public static readonly FakeClock Instance = new();

    public DateTime UtcNow => GetNow();

    public Func<DateTime> GetNow = () => TestData.Now;
}
