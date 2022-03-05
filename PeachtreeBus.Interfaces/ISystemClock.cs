using System;

namespace PeachtreeBus.Interfaces
{
    /// <summary>
    /// Defines an abstraction for the system clock.
    /// Supports testable code.
    /// </summary>
    public interface ISystemClock
    {
        DateTime UtcNow { get; }
    }

    /// <summary>
    /// A default implementation of ISystemClock
    /// </summary>
    public class SystemClock : ISystemClock
    {
        public DateTime UtcNow { get => DateTime.UtcNow; }
    }
}
