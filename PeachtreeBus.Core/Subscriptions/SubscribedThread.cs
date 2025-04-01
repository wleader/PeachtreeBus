using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using PeachtreeBus.Exceptions;
using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions
{
    /// <summary>
    /// Describes a task that will read subscribed messages and attempt to process them.
    /// </summary>
    public interface ISubscribedThread : IThread { }

    /// <summary>
    /// A task that will read subscribed messages and attempt to process them.
    /// </summary>
    public class SubscribedThread : BaseThread, ISubscribedThread
    {
        private readonly ISubscribedWork _subscribedWork;

        public SubscribedThread(
            ILogger<SubscribedThread> log,
            IBusDataAccess busDataAccess,
            IBusConfiguration configuration,
            ISubscribedWork subscribeWork)
            : base("Subscription Message", 100, log, busDataAccess)
        {
            _subscribedWork = subscribeWork;
            _subscribedWork.SubscriberId = UnreachableException.ThrowIfNull(configuration.SubscriptionConfiguration,
                message: "Subscription configuration is required to create a SubscribedThread")
                .SubscriberId;
        }

        public override async Task<bool> DoUnitOfWork()
        {
            return await _subscribedWork.DoWork();
        }
    }
}
