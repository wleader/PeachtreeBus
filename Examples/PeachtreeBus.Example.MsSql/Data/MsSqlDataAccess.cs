using Dapper;
using PeachtreeBus.DatabaseSharing;
using System.Threading.Tasks;

namespace PeachtreeBus.Example.Data
{
    /// <summary>
    /// A Data Access example that shares the database connection and transaction object
    /// with PeachtreeBus.
    /// </summary>
    public class MsSqlDataAccess(
        ISharedDatabase database)
        : IExampleDataAccess
    {
        public Task Audit(string message)
        {
            // This is in Microsoft SQL dialect
            const string statement = "INSERT INTO[ExampleApp].[AuditLog] ([Occured],[Message]) VALUES(SYSUTCDATETIME(),@Message)";
            var p = new DynamicParameters();
            p.Add("@Message", message);
            return database.NativeConnection.ExecuteAsync(statement, p, database.NativeTransaction);
        }
    }
}
