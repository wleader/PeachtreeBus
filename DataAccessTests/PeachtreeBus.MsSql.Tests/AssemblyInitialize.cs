using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.DatabaseTesting;
using PeachtreeBus.DatabaseTesting.MsSql;
using PeachtreeBus.Errors;
using PeachtreeBus.Management;
using PeachtreeBus.MsSql.Tests;
using PeachtreeBus.Serialization;

namespace PeachtreeBus.DataAccessTests;

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
            c.AddScoped<IManagementDataAccess, MsSqlManagementDataAccess>();
            c.AddScoped<IBusDataAccess, MsSqlBusDataAccess>();
            c.AddScoped<ISqlSharedDatabase, SharedDatabase>();
            c.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
            c.AddSingleton<IProvideDbConnectionString, ProvideDbConnectionString>();
            c.AddScoped<IDapperMethods, DapperMethods>();
            c.AddSingleton<IDapperTypesHandler, DapperTypesHandler>();
            c.AddSingleton<ISerializer, DefaultSerializer>();
            c.AddTransient<ICircuitBreakerProvider, FakeBreakerProvider>();
            c.AddTransient<ITestDataAccess, MsSqlTestDataAccess>();
            c.AddSingleton<ITestConfig, TestConfig>();
            c.AddSingleton<IInitializeTestDatabase, MsSqlInitializeDatabase>();
            c.AddSingleton<IDatabaseManagement, DatabaseManagement>();
            c.AddSingleton<IMsSqlTestSettings, MsSqlTestSettings>();
        });
        
        var initializer =  TestServices.GetService<IInitializeTestDatabase>();
        initializer.Initialize();
    }
}
