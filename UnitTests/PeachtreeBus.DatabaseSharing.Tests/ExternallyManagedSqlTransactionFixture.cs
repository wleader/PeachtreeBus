using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Testing;

namespace PeachtreeBus.DatabaseSharing.Tests;

[TestClass]
public class ExternallyManagedSqlTransactionFixture
{
    private ExternallyManagedSqlTransaction _transaction = default!;
    private SqlTransaction _nativeTransaction = default!;

    [TestInitialize]
    public void Intialize()
    {
        _nativeTransaction = SqlServerTesting.CreateTransaction();
        _transaction = new(_nativeTransaction);
    }

    [TestMethod]
    public void When_Disposed_Then_Disposed()
    {
        _transaction.Dispose();
        Assert.IsTrue(_transaction.Disposed);
        // unfortunately we can't check that the actual
        // SqlTransaction object wasn't disposed because its got no event or property we can read.
    }

    [TestMethod]
    public void When_Commit_Then_Throws()
    {
        Assert.ThrowsExactly<ExternallyManagedSqlConnectionException>(_transaction.Commit);
    }

    [TestMethod]
    public void When_Rollback_Then_Throws()
    {
        Assert.ThrowsExactly<ExternallyManagedSqlConnectionException>(_transaction.Rollback);
    }
}
