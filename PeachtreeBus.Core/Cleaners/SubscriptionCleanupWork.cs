using PeachtreeBus.Data;
using System.Threading.Tasks;

namespace PeachtreeBus.Cleaners
{
    /// <summary>
    /// Describes a unit of work that cleans up expired subscriptons.
    /// </summary>
    public interface ISubscriptionCleanupWork : IUnitOfWork { }

    /// <summary>
    /// A unit of work that cleans up expired subscriptions.
    /// </summary>
    public class SubscriptionCleanupWork(
        IBusDataAccess dataAccess,
        IBusConfiguration configuration)
        : ISubscriptionCleanupWork
    {
        private readonly IBusDataAccess _dataAccess = dataAccess;
        private readonly IBusConfiguration _configuration = configuration;

        public async Task<bool> DoWork()
        {
            if (_configuration.SubscriptionConfiguration is null) return false;

            await _dataAccess.ExpireSubscriptionMessages(_configuration.SubscriptionConfiguration.CleanMaxRows);
            await _dataAccess.ExpireSubscriptions(_configuration.SubscriptionConfiguration.CleanMaxRows);
            return true;
        }
    }
}
