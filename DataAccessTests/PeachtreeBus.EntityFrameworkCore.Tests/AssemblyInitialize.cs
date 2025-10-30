using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.DatabaseTestingShared;

namespace PeachtreeBus.EntityFrameworkCore.Tests;

[TestClass]
public static class AssemblyInitialize
{
    [AssemblyInitialize]
    public static void Initialize(TestContext _)
    {
        TestSettings.Initialize();
        DbInitialization.Initialize();
    }
}