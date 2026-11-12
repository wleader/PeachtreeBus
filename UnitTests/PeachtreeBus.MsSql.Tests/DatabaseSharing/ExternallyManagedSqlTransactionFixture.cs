using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Testing;

namespace PeachtreeBus.MsSql.Tests.DatabaseSharing;

[TestClass]
public class ExternallyManagedSqlTransactionFixture
{
    private ExternallyManagedSqlTransaction _transaction = null!;
    private SqlTransaction _nativeTransaction = null!;

    [TestInitialize]
    public void Initialize()
    {
        _nativeTransaction = UninitializedObjects.Create<SqlTransaction>();
        _transaction = new(_nativeTransaction);
    }

    [TestMethod]
    public void When_Disposed_Then_Disposed()
    {
        _transaction.Dispose();
        Assert.IsTrue(_transaction.Disposed);
        // unfortunately we can't check that the actual
        // SqlTransaction object wasn't disposed because it has no event or property we can read.
    }

    [TestMethod]
    public void When_Commit_Then_Throws()
    {
        Assert.ThrowsExactly<ExternallyManagedTransactionException>(_transaction.Commit);
    }

    [TestMethod]
    public void When_Rollback_Then_Throws()
    {
        Assert.ThrowsExactly<ExternallyManagedTransactionException>(_transaction.Rollback);
    }
}
