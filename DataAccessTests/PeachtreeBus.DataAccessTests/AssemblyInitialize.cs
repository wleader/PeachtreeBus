using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.DatabaseTesting;
using PeachtreeBus.DatabaseTesting.MsSql;

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
            c.AddSingleton<IInitializeTestDatabase, MsSqlInitializeDatabase>();
            c.AddSingleton<IMsSqlTestSettings, MsSqlTestSettings>();
            c.AddSingleton<IDatabaseManagement, DatabaseManagement>();
            c.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
            c.AddSingleton<IProvideDbConnectionString, ProvideDbConnectionString>();
        });
        
        var initializer =  TestServices.GetService<IInitializeTestDatabase>();
        initializer.Initialize();
    }
}
