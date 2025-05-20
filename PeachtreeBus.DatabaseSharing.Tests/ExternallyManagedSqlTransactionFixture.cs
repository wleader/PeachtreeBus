using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.CompilerServices;

namespace PeachtreeBus.DatabaseSharing.Tests;

[TestClass]
public class ExternallyManagedSqlTransactionFixture
{
    private ExternallyManagedSqlTransaction _transaction = default!;
    private SqlTransaction _nativeTransaction = default!;

    [TestInitialize]
    public void Intialize()
    {
        _nativeTransaction = GetUninitialzed<SqlTransaction>();

        _transaction = new(_nativeTransaction);
    }

    private static T GetUninitialzed<T>()
    {
        return (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
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
