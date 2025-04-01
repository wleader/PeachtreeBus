using PeachtreeBus.Data;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions
{
    /// <summary>
    /// Describes a unit of work that updates the subscriptions for a subscriber
    /// </summary>
    public interface ISubscriptionUpdateWork : IUnitOfWork
    {
        public DateTime LastUpdate { get; }
    }

    /// <summary>
    /// A unit of work that updates the subcriptions for the subscriber.
    /// </summary>
    public class SubscriptionUpdateWork(
        IBusDataAccess dataAccess,
        IBusConfiguration config,
        ISystemClock clock)
        : ISubscriptionUpdateWork
    {
        public DateTime LastUpdate { get; private set; } = DateTime.MinValue;

        private readonly IBusDataAccess _dataAccess = dataAccess;
        private readonly SubscriptionConfiguration _config = config.SubscriptionConfiguration!;
        private readonly ISystemClock _clock = clock;

        public async Task<bool> DoWork()
        {
            // make sure our data in the subscriptions table is up to date.
            var nextUpdate = LastUpdate.Add(_config.Lifespan / 2);
            if (_clock.UtcNow < nextUpdate) return false;

            var until = _clock.UtcNow.Add(_config.Lifespan);
            foreach (var topic in _config.Topics)
            {
                await _dataAccess.Subscribe(
                    _config.SubscriberId,
                    topic,
                    until);
            }
            LastUpdate = _clock.UtcNow;
            return true;
        }
    }
}
