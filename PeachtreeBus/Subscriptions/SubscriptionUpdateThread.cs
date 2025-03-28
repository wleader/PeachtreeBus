﻿using Microsoft.Extensions.Logging;
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
    public class SubscriptionUpdateThread(
        ILogger<SubscriptionUpdateThread> log,
        IBusDataAccess transactionContext,
        ISubscriptionUpdateWork updater)
        : BaseThread("Subscription Update", 500, log, transactionContext)
        , ISubscriptionUpdateThread
    {
        private readonly ISubscriptionUpdateWork _updater = updater;

        public override async Task<bool> DoUnitOfWork()
        {
            return await _updater.DoWork();
        }
    }
}
