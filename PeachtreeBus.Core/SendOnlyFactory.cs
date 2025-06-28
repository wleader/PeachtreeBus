using Microsoft.Data.SqlClient;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System;

namespace PeachtreeBus;

public interface ISendOnlyFactory
{
    public IQueueWriter CreateQueueWriter(IServiceProvider serviceProvider, SqlConnection connection, SqlTransaction? transaction);
    public ISubscribedPublisher CreateSubscribedPublisher(IServiceProvider serviceProvider, SqlConnection connection, SqlTransaction? transaction);
}

public class SendOnlyFactory
    : ISendOnlyFactory
{
    private IServiceProviderAccessor? _accessor = null;
    private readonly object LockObj = new();

    private void ConfigureAccessorOnce(IServiceProvider serviceProvider, SqlConnection connection, SqlTransaction? transaction)
    {
        lock (LockObj)
        {
            if (_accessor is not null)
                return;

            _accessor = serviceProvider.GetRequiredService<IServiceProviderAccessor>();

            if (!_accessor.IsConfigured)
                _accessor.UseExisting(serviceProvider);

            var sharedDb = serviceProvider.GetRequiredService<ISharedDatabase>();
            sharedDb.SetExternallyManagedConnection(connection, transaction);
            var shareBetweenScopes = serviceProvider.GetRequiredService<IShareObjectsBetweenScopes>();
            shareBetweenScopes.SharedDatabase = sharedDb;
        }
    }

    public IQueueWriter CreateQueueWriter(IServiceProvider serviceProvider, SqlConnection connection, SqlTransaction? transaction)
    {
        ConfigureAccessorOnce(serviceProvider, connection, transaction);
        return _accessor!.ServiceProvider.GetRequiredService<IQueueWriter>();
    }

    public ISubscribedPublisher CreateSubscribedPublisher(IServiceProvider serviceProvider, SqlConnection connection, SqlTransaction? transaction)
    {
        ConfigureAccessorOnce(serviceProvider, connection, transaction);
        return _accessor!.ServiceProvider.GetRequiredService<ISubscribedPublisher>();
    }
}
