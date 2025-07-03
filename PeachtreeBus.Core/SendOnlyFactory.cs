using Microsoft.Data.SqlClient;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System;

namespace PeachtreeBus;

/// <summary>
/// An interface for code that creates an IQueueWriter or ISubscribedPulisher that is properly configured
/// to use an existing Database Connection, and creates all needed dependencies within an existing scope.
/// </summary>
public interface ISendOnlyFactory
{
    /// <summary>
    /// Creates an IQueueWriter using an existing scope, and database connection.
    /// </summary>
    /// <param name="serviceProvider">The existing scope to use when creating objects. This must be an IServiceProvider that is scoped.</param>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <returns></returns>
    public IQueueWriter CreateQueueWriter(IServiceProvider serviceProvider, SqlConnection connection, SqlTransaction? transaction);

    /// <summary>
    /// Creates an ISubscribedPublisher using an existing scope, and database connection.
    /// </summary>
    /// <param name="serviceProvider">The existing scope to use when creating objects. This must be an IServiceProvider that is scoped.</param>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    public ISubscribedPublisher CreateSubscribedPublisher(IServiceProvider serviceProvider, SqlConnection connection, SqlTransaction? transaction);
}

/// <inheritdoc/>
public class SendOnlyFactory
    : ISendOnlyFactory
{
    private IServiceProviderAccessor? _accessor = null;
    private readonly object LockObj = new();

    private void SetupOnce(IServiceProvider serviceProvider, SqlConnection connection, SqlTransaction? transaction)
    {
        // use a lock just in cas
        lock (LockObj)
        {
            if (_accessor is not null)
                return;

            _accessor = serviceProvider.GetRequiredService<IServiceProviderAccessor>();

            if (!_accessor.IsConfigured)
                _accessor.UseExisting(serviceProvider);

            var shareBetweenScopes = serviceProvider.GetRequiredService<IShareObjectsBetweenScopes>();
            var sharedDb = serviceProvider.GetRequiredService<ISharedDatabase>();

            sharedDb.SetExternallyManagedConnection(connection, transaction);
            shareBetweenScopes.SharedDatabase = sharedDb;
        }
    }

    /// <inheritdoc/>
    public IQueueWriter CreateQueueWriter(IServiceProvider serviceProvider, SqlConnection connection, SqlTransaction? transaction)
    {
        SetupOnce(serviceProvider, connection, transaction);
        return _accessor!.GetRequiredService<IQueueWriter>();
    }

    /// <inheritdoc/>
    public ISubscribedPublisher CreateSubscribedPublisher(IServiceProvider serviceProvider, SqlConnection connection, SqlTransaction? transaction)
    {
        SetupOnce(serviceProvider, connection, transaction);
        return _accessor!.GetRequiredService<ISubscribedPublisher>();
    }
}
