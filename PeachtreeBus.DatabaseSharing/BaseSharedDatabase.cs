using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.DatabaseSharing;

public interface ISharedDatabase : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Enables preventing the Shared Database object from being disposed.
    /// This is useful because the object has to be accessible as a scoped object
    /// in multiple DI Scopes, but the DI container must not dispose it when
    /// and inner scope ends.
    /// </summary>
    bool DenyDispose { get; set; }
    
    bool Disposed { get; }

    /// <summary>
    /// Starts a transaction. Only one transaction should be started.
    /// Causes the TransactionStarted event.
    /// </summary>
    void BeginTransaction();

    /// <summary>
    /// Commits a transaction.
    /// Causes the TransactionConsumed event.
    /// </summary>
    void CommitTransaction();

    /// <summary>
    /// Rolls back a transaction.
    /// Causes the TransactionConsumed event.
    /// </summary>
    void RollbackTransaction();

    /// <summary>
    /// Closes and reopens the database connection.
    /// </summary>
    void Reconnect();

    /// <summary>
    /// Creates a Savepoint in the Transaction.
    /// Similar to a nested transaction.
    /// </summary>
    /// <param name="name">The name of the savepoint to create.</param>
    void CreateSavepoint(string name);

    /// <summary>
    /// Rolls back to Savepoint in the transaction.
    /// Similar to a nested transaction rollboack.
    /// </summary>
    /// <param name="name">The name of the savepoint to rollback to.</param>
    void RollbackToSavepoint(string name);

    /// <summary>
    /// An event to signal a new transaction so that other DbContext instances can synchronize transaction usage.
    /// </summary>
    event EventHandler TransactionStarted;

    /// <summary>
    /// An event to signal a transaction ended so that other DbContext instances can synchronize transaction usage.
    /// </summary>
    event EventHandler TransactionConsumed;
    
    void SetExternallyManagedConnection(DbConnection connection, DbTransaction? transaction);
}

public interface IBaseSharedDatabase<out TConnection, out TTransaction> : ISharedDatabase
{
    /// <summary>
    /// The current Transaction (Null when there is no transaction).
    /// </summary>
    TTransaction? Transaction { get; }

    TConnection Connection { get; }
}

public abstract class BaseSharedDatabase<TConnection, TConnectionInterface, TTransaction, TTransactionInterface>(
    IDbConnectionFactory<TConnectionInterface> connectionFactory)
    : IBaseSharedDatabase<TConnection, TTransaction>
    where TConnection : DbConnection, IDisposable, IAsyncDisposable
    where TConnectionInterface : class, IBaseConnection<TConnection, TTransaction, TTransactionInterface>
    where TTransaction : DbTransaction, IDisposable, IAsyncDisposable
    where TTransactionInterface : class, IBaseTransaction<TTransaction>
{
    /// <summary>
    /// Give each instance a different ID.
    /// Helps with diagnosing which instances are the same and different.
    /// </summary>
    public Guid InstanceId { get; } = Guid.NewGuid();

    /// <inheritdoc/>
    public bool DenyDispose { get; set; }
    
    private long _disposed;
    public bool Disposed => Interlocked.Read(ref _disposed) != 0;

    /// <inheritdoc/>
    public event EventHandler? TransactionStarted;

    /// <inheritdoc/>
    public event EventHandler? TransactionConsumed;

    private void OnTransactionConsumed() => TransactionConsumed?.Invoke(this, EventArgs.Empty);
    private void OnTransactionStarted() => TransactionStarted?.Invoke(this, EventArgs.Empty);

    private TConnectionInterface? _connection;
    private TTransactionInterface? _transaction;
    private readonly Lock _lock = new();
    private bool _connectionIsExternallyManaged;

    public TConnection Connection
    {
        get
        {
            lock (_lock)
            {
                _connection ??= connectionFactory.GetConnection();
                return _connection.Connection;
            }
        }
    }
    
    /// <inheritdoc/>
    public TTransaction? Transaction => _transaction?.Transaction;

    /// <inheritdoc/>
    public void BeginTransaction()
    {
        lock (_lock)
        {
            _connection ??= connectionFactory.GetConnection();
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                Reconnect();
            }

            if (_transaction != null)
                throw new SharedDatabaseException(
                    "There is already a transaction. Use CreateSavePoint instead of nested transactions.");

            _transaction = _connection.BeginTransaction();
        }

        OnTransactionStarted();
    }

    /// <inheritdoc/>
    public void CommitTransaction()
    {
        lock (_lock)
        {
            if (_transaction == null)
                throw new SharedDatabaseException("There is no transaction to commit.");

            _transaction.Commit();
            _transaction = null;
        }

        OnTransactionConsumed();
    }

    /// <inheritdoc/>
    public void CreateSavepoint(string name)
    {
        lock (_lock)
        {
            if (_transaction == null)
                throw new SharedDatabaseException("There is no transaction to create a save point in.");

            _transaction.Save(name);
        }
    }
    
    /// <inheritdoc/>
    public void RollbackToSavepoint(string name)
    {
        lock (_lock)
        {
            if (_transaction == null) throw new SharedDatabaseException("There is no transaction to roll back to a save point.");
            _transaction.Rollback(name);
        }
    }

    /// <inheritdoc/>
    public void RollbackTransaction()
    {
        lock (_lock)
        {
            if (_transaction == null)
                throw new SharedDatabaseException("There is no transaction to roll back.");

            _transaction.Rollback();
            _transaction = null;
        }
        OnTransactionConsumed();
    }
    
    /// <inheritdoc/>
    public void Reconnect()
    {
        if (_connectionIsExternallyManaged)
            throw new ExternallyManagedConnectionException(
                "Reconnection is not allowed when using an Externally Managed Connection.");

        if (DenyDispose)
            throw new SharedDatabaseException("Reconnection is not allowed when DenyDispose is true.");

        lock (_lock)
        {
            DisposeTransactionAndConnection();
            _connection = connectionFactory.GetConnection();
            _connection.Open();
        }
    }

    private void DisposeTransactionAndConnection()
    {
        // this code can call .Dispose
        // even if the transaction and connection are 
        // externally managed. That is because the 
        // external classes do not pass the dispose call
        // through to the native objects.

        if (_transaction is not null)
        {
            _transaction.Dispose();
            _transaction = null;
            OnTransactionConsumed();
        }

        _connection?.Dispose();
        _connection = null;
    }

    private async ValueTask DisposeTransactionAndConnectionAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
            OnTransactionConsumed();
        }

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    protected abstract TConnectionInterface CreateExternalConnection(DbConnection  connection);
    protected abstract TTransactionInterface CreateExternalTransaction(DbTransaction transaction);
    
    public void SetExternallyManagedConnection(DbConnection connection, DbTransaction? transaction)
    {
        lock (_lock)
        {
            DisposeTransactionAndConnection();
            _connectionIsExternallyManaged = true;
            _connection = CreateExternalConnection(connection);
            if (transaction is not null)
                _transaction = CreateExternalTransaction(transaction);
        }
    }
    
    public void Dispose()
    {
        if (DenyDispose) return;
        if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0) return;
        DisposeTransactionAndConnection();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (DenyDispose) return;
        if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0) return;
        await DisposeTransactionAndConnectionAsync();
        GC.SuppressFinalize(this);
    }
}