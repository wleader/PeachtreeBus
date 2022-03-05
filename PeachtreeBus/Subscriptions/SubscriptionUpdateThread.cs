using PeachtreeBus.Data;
using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions
{
    /// <summary>
    /// Describes a thread that that keeps the subscribers subscriptions updated.
    /// </summary>
    public interface ISubscriptionUpdateThread : IThread { }

    /// <summary>
    /// A thread that keeps the subscribers subscriptions updated.
    /// </summary>
    public class SubscriptionUpdateThread : BaseThread, ISubscriptionUpdateThread
    {
        private readonly ISubscriptionUpdateWork _updater;

        public SubscriptionUpdateThread(IProvideShutdownSignal shutdown,
            ILog<SubscriptionUpdateThread> log,
            IBusDataAccess transactionContext,
            ISubscriptionUpdateWork updater)
            : base("Subscription Update", 500, log, transactionContext, shutdown)
        {
            _updater = updater;
        }

        public override Task<bool> DoUnitOfWork()
        {
            return _updater.DoWork();
        }
    }
}
