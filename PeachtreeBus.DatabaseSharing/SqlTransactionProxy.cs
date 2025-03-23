using Microsoft.Data.SqlClient;
using System;
using System.Diagnostics.CodeAnalysis;

namespace PeachtreeBus.DatabaseSharing
{
    /// <summary>
    /// Implements the ISqlTransaction interface by passing
    /// directly though to Microsoft.Data.SqlClient.SqlTransaction.
    /// </summary>
    /// <param name="transaction"></param>
    [ExcludeFromCodeCoverage(Justification =
        "This object requires a connection to a real SQL server to test it.")]
    public class SqlTransactionProxy(SqlTransaction transaction) : ISqlTransaction
    {
        public bool Disposed { get; private set; } = false;
        public SqlTransaction Transaction { get; } = transaction;
        public void Commit() => Transaction.Commit();
        public void Rollback() => Transaction.Rollback();
        public void Rollback(string transactionName) => Transaction.Rollback(transactionName);
        public void Save(string savePointName) => Transaction.Save(savePointName);

        public void Dispose()
        {
            Transaction.Dispose();
            GC.SuppressFinalize(this);
            Disposed = true;
        }
    }
}
