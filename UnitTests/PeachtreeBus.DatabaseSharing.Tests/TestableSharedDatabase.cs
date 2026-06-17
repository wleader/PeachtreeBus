using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.DatabaseSharing.Tests;

public interface ITestableTransaction : IBaseTransaction<DbTransaction>;

public interface ITestableConnection : IBaseConnection<DbConnection, DbTransaction, ITestableTransaction>;

public class TestableConnection(DbConnection connection)
    : BaseConnection<DbConnection, DbTransaction, IBaseTransaction<DbTransaction>>(connection)
{
    public override IBaseTransaction<DbTransaction> BeginTransaction()
    {
        throw new System.NotImplementedException();
    }

    public override Task<IBaseTransaction<DbTransaction>> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }
}

public interface ITestableConnectionFactory : IDbConnectionFactory<ITestableConnection>;

public class TestableSharedDatabase(ITestableConnectionFactory connectionFactory)
    : BaseSharedDatabase<DbConnection, ITestableConnection, DbTransaction, ITestableTransaction>(connectionFactory)
{
    protected override ITestableConnection CreateExternalConnection(DbConnection connection) => 
        new ExternallyManagedTestableConnection(connection);

    protected override ITestableTransaction CreateExternalTransaction(DbTransaction transaction) => 
        new ExternallyManagedTestableTransaction(transaction);
}

public class ExternallyManagedTestableConnection(DbConnection connection) 
    : ExternallyManagedConnection<DbConnection, DbTransaction, ITestableTransaction>(connection)
    , ITestableConnection;
    
public class ExternallyManagedTestableTransaction(DbTransaction transaction) 
    : ExternallyManagedTransaction<DbTransaction>(transaction)
    , ITestableTransaction;