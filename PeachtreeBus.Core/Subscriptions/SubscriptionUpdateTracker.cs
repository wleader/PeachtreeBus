using PeachtreeBus.Tasks;

namespace PeachtreeBus.Subscriptions;

public interface ISubscriptionUpdateTracker : INextRunTracker;

public class SubscriptionUpdateTracker(ISystemClock clock, IBusConfiguration config)
    : NextRunTracker(clock, config.SubscriptionConfiguration is null
        ? null
        : config.SubscriptionConfiguration.Lifespan / 2)
    , ISubscriptionUpdateTracker;

