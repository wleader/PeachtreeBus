using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.DatabaseTestingShared;

namespace PeachtreeBus.DataAccessTests;

[TestClass]
public static class AssemblyInitialize
{
    [AssemblyInitialize]
    public static void Initialize(TestContext _)
    {
        AppSettings.Initialize();
        DbInitialization.Initialize();
    }
}
