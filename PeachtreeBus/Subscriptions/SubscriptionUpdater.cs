using PeachtreeBus.Data;
using PeachtreeBus.Interfaces;
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
    public class SubscriptionUpdateWork : ISubscriptionUpdateWork
    {
        public DateTime LastUpdate { get; private set; } = DateTime.MinValue;

        private readonly IBusDataAccess _dataAccess;
        private readonly ISubscriberConfiguration _config;
        private readonly ISystemClock _clock;

        public SubscriptionUpdateWork(
            IBusDataAccess dataAccess,
            ISubscriberConfiguration config,
            ISystemClock clock)
        {
            _dataAccess = dataAccess;
            _config = config;
            _clock = clock;
        }

        public async Task<bool> DoWork()
        {
            // make sure our data in the subscriptions table is up to date.
            var nextUpdate = LastUpdate.Add(_config.Lifespan / 2);
            if (_clock.UtcNow < nextUpdate) return false;

            var until = _clock.UtcNow.Add(_config.Lifespan);
            foreach (var category in _config.Categories)
            {
                await _dataAccess.Subscribe(
                    _config.SubscriberId,
                    category,
                    until);
            }
            LastUpdate = _clock.UtcNow;
            return true;
        }
    }
}
