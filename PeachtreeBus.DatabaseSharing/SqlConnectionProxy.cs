using Microsoft.Data.SqlClient;
using System;
using System.Diagnostics.CodeAnalysis;

namespace PeachtreeBus.DatabaseSharing
{
    /// <summary>
    /// Implements the ISqlConnection interface by passing
    /// directly though to Microsoft.Data.SqlClient.SqlConnection.
    /// </summary>
    /// <param name="connectionString"></param>
    [ExcludeFromCodeCoverage(Justification =
        "This object requires a real SQL server to properly test.")]
    public class SqlConnectionProxy(string connectionString) : ISqlConnection
    {
        public bool Disposed { get; private set; } = false;
        public SqlConnection Connection { get; } = new(connectionString);
        public System.Data.ConnectionState State { get => Connection.State; }

        public void Open() => Connection.Open();
        public void Close() => Connection.Close();

        public ISqlTransaction BeginTransaction()
        {
            return new SqlTransactionProxy(Connection.BeginTransaction());
        }

        public void Dispose()
        {
            Connection.Dispose();
            GC.SuppressFinalize(this);
            Disposed = true;
        }
    }
}
