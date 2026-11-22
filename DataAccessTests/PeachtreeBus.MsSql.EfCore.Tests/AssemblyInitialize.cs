using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.DatabaseTesting;
using PeachtreeBus.DatabaseTesting.MsSql;

namespace PeachtreeBus.EntityFrameworkCore.Tests;

[TestClass]
public static class AssemblyInitialize
{
    [AssemblyInitialize]
    public static void Initialize(TestContext _)
    {
        TestServices.Initialize(c =>
        {
            c.AddSingleton<IInitializeTestDatabase, MsSqlInitializeDatabase>();
            c.AddSingleton<IMsSqlTestSettings, MsSqlTestSettings>();
            c.AddSingleton<IManageTestDatabase, MsSqlDatabaseManagement>();
            c.AddTransient<ITestSettings>(sp => sp.GetRequiredService<IMsSqlTestSettings>());
        });

        var initializer =  TestServices.GetService<IInitializeTestDatabase>();
        initializer.Initialize();
    }
    
    [AssemblyCleanup]
    public static void Cleanup(TestContext _)
    {
        var initializer =  TestServices.GetService<IInitializeTestDatabase>();
        initializer.Cleanup();
    }
}