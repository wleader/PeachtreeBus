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
        ISubscribedCleanupConfiguration configuration)
        : ISubscriptionCleanupWork
    {
        private readonly IBusDataAccess _dataAccess = dataAccess;
        private readonly ISubscribedCleanupConfiguration configuration = configuration;

        public async Task<bool> DoWork()
        {
            await _dataAccess.ExpireSubscriptionMessages(configuration.MaxDeleteCount);
            await _dataAccess.ExpireSubscriptions(configuration.MaxDeleteCount);
            return true;
        }
    }
}
