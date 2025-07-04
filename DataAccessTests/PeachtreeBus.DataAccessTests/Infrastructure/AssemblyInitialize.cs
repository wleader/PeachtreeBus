using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PeachtreeBus.DataAccessTests.Infrastructure;

[TestClass]
public static class AssemblyInitialize
{
    [AssemblyInitialize]
    public static void Initialize(TestContext _)
    {
        AppSettings.Initialize();
        DbInitialization.Intitialize();
    }
}
