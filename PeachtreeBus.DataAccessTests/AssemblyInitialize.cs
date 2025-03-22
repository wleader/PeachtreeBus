using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Dac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace PeachtreeBus.DataAccessTests
{
    [TestClass]
    public class AssemblyInitialize
    {
        private static readonly string serverConnectionString;

        private static readonly string testDbName;

        static AssemblyInitialize()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("appsettings.json");
            var config = configurationBuilder.Build();

            TestConfig.DbConnectionString = config.GetConnectionString("TestDatabase")
                ?? throw new ApplicationException("Connection string not configured.");

            var csb = new SqlConnectionStringBuilder(TestConfig.DbConnectionString);
            testDbName = csb.InitialCatalog;
            csb.InitialCatalog = "";
            serverConnectionString = csb.ConnectionString;
        }

        [AssemblyInitialize]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter",
            Justification = "Required by Unit Testing Runtime.")]
        public static void AssemblyInit(TestContext context)
        {
            Assert.IsNotNull(serverConnectionString);
            Assert.IsNotNull(TestConfig.DbConnectionString);

            var connection = new SqlConnection(serverConnectionString);
            connection.Open();
            Assert.AreEqual(System.Data.ConnectionState.Open, connection.State);

            // delete the previous database.            
            if (DbExists(connection))
            {
                DropDatabase(connection);
            }

            // create a new database.
            CreateDatabase(connection);
            Assert.IsTrue(DbExists(connection));


            // create schema and tables

            // find the newest dacpac (debug or release);
            var searchpath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "..", "PeachtreeBus.Database"));
            var newestDacPac = Directory.GetFiles(searchpath, "PeachtreeBus.Database.dacpac", SearchOption.AllDirectories)
                .Select(filename => new Tuple<string, DateTime>(filename, File.GetLastWriteTime(filename)))
                .OrderByDescending(t => t.Item2).First().Item1;

            Assert.IsNotNull(newestDacPac);

            var dacpac = DacPackage.Load(newestDacPac);
            var dacService = new DacServices(TestConfig.DbConnectionString);
            dacService.Deploy(dacpac, testDbName, true);

            // no errors thrown?
        }

        private static bool DbExists(SqlConnection connection)
        {
            var command = new SqlCommand($"SELECT database_id FROM sys.databases WHERE Name = '{testDbName}'", connection);
            var result = command.ExecuteScalar();
            return (result != null);
        }

        private static void CreateDatabase(SqlConnection connection)
        {
            var command = new SqlCommand($"CREATE DATABASE {testDbName}", connection);
            command.ExecuteNonQuery();
        }

        private static void DropDatabase(SqlConnection connection)
        {
            var command = new SqlCommand($"alter database [{testDbName}] set single_user with rollback immediate; DROP DATABASE {testDbName}", connection);
            command.ExecuteNonQuery();
        }

    }
}
