using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseTesting;
using PeachtreeBus.DatabaseTesting.PostgreSql;

namespace PeachtreeBus.DatabaseSharing.PostgreSql.Tests;

[TestClass]
public static class AssemblyInitialize
{
    [AssemblyInitialize]
    public static void Initialize(TestContext _)
    {
        TestServices.Initialize(c =>
        {
            // is something needed here?
            c.AddSingleton<IInitializeTestDatabase, PostgresInitializeTestDatabase>();
            c.AddSingleton<INpgSqlConnectionFactory, NpgSqlConnectionFactory>();
            c.AddSingleton<IProvideDbConnectionString, ProvideDbConnectionString>();
            c.AddSingleton<IPostgreSqlTestSettings, PostgreSqlTestSettings>();
            c.AddTransient<ITestSettings>(sp => sp.GetRequiredService<IPostgreSqlTestSettings>());
            c.AddSingleton<IManageTestDatabase, PostgresManageTestDatabase>();
            c.AddScoped<NpgSqlSharedDatabase>();
        });

        var initializer =  TestServices.GetService<IInitializeTestDatabase>();
        initializer.Initialize();
    }
}
