using PeachtreeBus.Tasks;

namespace PeachtreeBus.Subscriptions;

public interface ICleanSubscriptionsTracker : INextRunTracker;

public class CleanSubscriptionsTracker(
    ISystemClock clock,
    IBusConfiguration config)
    : NextRunTracker(clock, config.SubscriptionConfiguration?.CleanInterval)
    , ICleanSubscriptionsTracker;

