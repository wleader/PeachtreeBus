using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Data;
using PeachtreeBus.DataAccessTests;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.DatabaseSharing.PostgreSql;
using PeachtreeBus.DatabaseTesting;
using PeachtreeBus.DatabaseTesting.PostgreSql;
using PeachtreeBus.Errors;
using PeachtreeBus.Serialization;

namespace PeachtreeBus.PostgreSql.Tests;

[TestClass]
public static class AssemblyInitialize
{
    [AssemblyInitialize]
    public static void Initialize(TestContext _)
    {
        TestServices.Initialize(c =>
        {
            // is something needed here?
            c.AddFakeLogging();
            //c.AddTransient<IMsSqlTestConnectionFactory, MsSqlTestConnectionFactory>();
            //c.AddSingleton<IMsSqlTestSettings, MsSqlTestSettings>();
            c.AddScoped<IBusDataAccess, PostgreSqlBusDataAccess>();
            c.AddScoped<INpgSqlSharedDatabase, NpgSqlSharedDatabase>();
            c.AddSingleton<INpgSqlConnectionFactory, NpgSqlConnectionFactory>();
            //c.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
            c.AddSingleton<IProvideDbConnectionString, ProvideDbConnectionString>();
            c.AddScoped<IDapperMethods, NpgSqlDapperMethods>();
            c.AddSingleton<IDapperTypesHandler, NpgSqlDapperTypesHandler>();
            c.AddSingleton<ISerializer, DefaultSerializer>();
            c.AddTransient<ICircuitBreakerProvider, FakeBreakerProvider>();
            c.AddTransient<ITestDataAccess, PostgreSqlTestDataAccess>();
            c.AddSingleton<ITestConfig, TestConfig>();
            c.AddSingleton<IInitializeTestDatabase, PostgresInitializeTestDatabase>();
        });

        var initializer =  TestServices.GetService<IInitializeTestDatabase>();
        initializer.Initialize();
    }
}
