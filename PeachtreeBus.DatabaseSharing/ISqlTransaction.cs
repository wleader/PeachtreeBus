using Microsoft.Data.SqlClient;
using System;

namespace PeachtreeBus.DatabaseSharing
{
    /// <summary>
    /// A mockable interface around the SQL Transaction
    /// to facilitate testing.
    /// </summary>
    public interface ISqlTransaction : IDisposable
    {
        bool Disposed { get; }
        SqlTransaction Transaction { get; }
        void Commit();
        void Rollback();
        void Rollback(string transactionName);
        void Save(string savePointName);
    }
}
