﻿using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
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

        public SubscribedThread(IProvideShutdownSignal provideShutdownSignal,
            ILogger<SubscribedThread> log,
            IBusDataAccess busDataAccess,
            ISubscriberConfiguration subscriberConfiguration,
            ISubscribedWork subscribeWork)
            : base("Subscription Message", 100, log, busDataAccess, provideShutdownSignal)
        {
            _subscribedWork = subscribeWork;
            _subscribedWork.SubscriberId = subscriberConfiguration.SubscriberId;
        }

        public override async Task<bool> DoUnitOfWork()
        {
            return await _subscribedWork.DoWork();
        }
    }
}
