using PeachtreeBus.Data;

namespace PeachtreeBus.DataAccessTests;

public abstract class BusDataAccessFixtureBase : DataAccessFixtureBase<IBusDataAccess>
{
    public override void Initialize()
    {
        base.Initialize();
        BusDataAccess.Reconnect();
    }
}