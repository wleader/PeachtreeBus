using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Dac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace PeachtreeBus.DataAccessTests
{
    [TestClass]
    class AssemblyInitialize
    {
        private static string serverConnectionString;
        public static string dbConnectionString;
        private static string testDbName;

        static AssemblyInitialize()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("appsettings.json");
            var config = configurationBuilder.Build();

            dbConnectionString = config.GetConnectionString("TestDatabase");

            var csb = new SqlConnectionStringBuilder(dbConnectionString);
            testDbName = csb.InitialCatalog;
            csb.InitialCatalog = "";
            serverConnectionString = csb.ConnectionString;
        }

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            Assert.IsNotNull(serverConnectionString);
            Assert.IsNotNull(dbConnectionString);

            var connection = new SqlConnection(serverConnectionString);
            connection.Open();
            Assert.AreEqual(System.Data.ConnectionState.Open, connection.State);

            // delete the previous database.            
            if (DbExists(connection, testDbName))
            {
                DropDatabase(connection, testDbName);
            }

            // create a new database.
            CreateDatabase(connection, testDbName);
            Assert.IsTrue(DbExists(connection, testDbName));


            // create schema and tables

            // find the newest dacpac (debug or release);
            var searchpath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "..","PeachtreeBus.Database"));
            var newestDacPac = Directory.GetFiles(searchpath, "PeachtreeBus.Database.dacpac", SearchOption.AllDirectories)
                .Select(filename => new Tuple<string, DateTime>(filename, File.GetLastWriteTime(filename)))
                .OrderByDescending(t => t.Item2).First().Item1;

            Assert.IsNotNull(newestDacPac);

            var dacpac = DacPackage.Load(newestDacPac);
            var dacService = new DacServices(dbConnectionString);
            dacService.Deploy(dacpac, testDbName, true);

            // no errors thrown?
        }

        private static bool DbExists(SqlConnection connection, string name)
        {
            var command = new SqlCommand($"SELECT database_id FROM sys.databases WHERE Name = '{testDbName}'", connection);
            var result = command.ExecuteScalar();
            return (result != null);
        }

        private static void CreateDatabase(SqlConnection connection, string name)
        {
            var command = new SqlCommand($"CREATE DATABASE {testDbName}", connection);
            command.ExecuteNonQuery();
        }

        private static void DropDatabase(SqlConnection connection, string name)
        {
            var command = new SqlCommand($"DROP DATABASE {testDbName}", connection);
            command.ExecuteNonQuery();
        }

    }
}
