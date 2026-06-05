using PeachtreeBus.Data;
using PeachtreeBus.DatabaseTesting;

namespace PeachtreeBus.DataAccessTests;

public abstract class BusDataAccessFixtureBase
{
    protected ITestDataAccess TestDataAccess { get; private set; } = null!;
    protected IBusDataAccess BusDataAccess { get; private set; } = null!;
    protected ITestConfig TestConfig { get; private set; } = null!;

    public virtual void Initialize()
    {
        TestConfig = TestServices.GetService<ITestConfig>();
        BusDataAccess = TestServices.GetService<IBusDataAccess>();
        TestDataAccess = TestServices.GetService<ITestDataAccess>();
        TestDataAccess.Initialize();
        TestDataAccess.CleanEverything();
    }

    public virtual void Cleanup()
    {
        TestDataAccess.CleanEverything();
        TestDataAccess.CloseConnections();
    }
}