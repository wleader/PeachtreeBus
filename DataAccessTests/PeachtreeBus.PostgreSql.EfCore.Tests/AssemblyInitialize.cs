using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.DatabaseTesting;
using PeachtreeBus.DatabaseTesting.PostgreSql;

namespace PeachtreeBus.PostgreSql.EfCore.Tests;

[TestClass]
public static class AssemblyInitialize
{
    [AssemblyInitialize]
    public static void Initialize(TestContext _)
    {
        TestServices.Initialize(c =>
        {
            c.AddSingleton<IInitializeTestDatabase, PostgresInitializeTestDatabase>();
            c.AddSingleton<IPostgreSqlTestSettings, PostgreSqlTestSettings>();
            c.AddSingleton<IManageTestDatabase, PostgresManageTestDatabase>();
            c.AddTransient<ITestSettings>(sp => sp.GetRequiredService<IPostgreSqlTestSettings>());
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