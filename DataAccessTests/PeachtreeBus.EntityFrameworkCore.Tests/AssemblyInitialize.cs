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
            c.AddSingleton<IDatabaseManagement, DatabaseManagement>();
        });

        var initializer =  TestServices.GetService<IInitializeTestDatabase>();
        initializer.Initialize();
    }
}