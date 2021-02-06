using Dapper;
using PeachtreeBus.DatabaseSharing;
using System.Threading.Tasks;

namespace PeachtreeBus.Example.Data
{
    /// <summary>
    /// An interface for the example application data access.
    /// </summary>
    public interface IExampleDataAccess
    {
        Task Audit(string message);
    }

    /// <summary>
    /// A Data Access example that share the database connection and transaction object with PeachtreeBus.
    /// </summary>
    public class ExampleDataAccess : IExampleDataAccess
    {
        private readonly ISharedDatabase _database;

        public ExampleDataAccess(ISharedDatabase database)
        {
            // get a shared database connection and transaction from the DI Container.
            // This will be the same connection and transaction used by PeachtreeBus.
            _database = database;
        }

        public Task Audit(string message)
        {
            const string statement = "INSERT INTO[ExampleApp].[AuditLog] ([Occured],[Message]) VALUES(SYSUTCDATETIME(),@Message)";
            var p = new DynamicParameters();
            p.Add("@Message", message);
            return _database.Connection.ExecuteAsync(statement, p, _database.Transaction);
        }
    }
}
