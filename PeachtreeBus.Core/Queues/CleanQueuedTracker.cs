using PeachtreeBus.Tasks;

namespace PeachtreeBus.Queues;

public interface ICleanQueuedTracker : INextRunTracker;

public class CleanQueuedTracker(
    ISystemClock clock,
    IBusConfiguration config)
    : NextRunTracker(clock, config.QueueConfiguration?.CleanInterval)
    , ICleanQueuedTracker;
