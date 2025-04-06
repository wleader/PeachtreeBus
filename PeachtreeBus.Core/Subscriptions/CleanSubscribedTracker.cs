using PeachtreeBus.Tasks;

namespace PeachtreeBus.Subscriptions;

public interface ICleanSubscribedTracker : INextRunTracker;

public class CleanSubscribedTracker(
    IBusConfiguration config,
    ISystemClock clock)
    : NextRunTracker(clock, config.SubscriptionConfiguration?.CleanInterval)
    , ICleanSubscribedTracker;
