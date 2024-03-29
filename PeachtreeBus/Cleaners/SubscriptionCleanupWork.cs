﻿using PeachtreeBus.Data;
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
        IBusDataAccess dataAccess)
        : ISubscriptionCleanupWork
    {
        private readonly IBusDataAccess _dataAccess = dataAccess;

        public async Task<bool> DoWork()
        {
            await _dataAccess.ExpireSubscriptions();
            return true;
        }
    }
}
