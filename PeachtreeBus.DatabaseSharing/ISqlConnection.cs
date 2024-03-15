using Microsoft.Data.SqlClient;
using System;

namespace PeachtreeBus.DatabaseSharing
{
    /// <summary>
    /// An interface around the SQL Connection
    /// to facilitate testing.
    /// </summary>
    public interface ISqlConnection : IDisposable
    {
        bool Disposed { get; }
        SqlConnection Connection { get; }
        System.Data.ConnectionState State { get; }
        void Open();
        ISqlTransaction BeginTransaction();
        void Close();
    }
}
