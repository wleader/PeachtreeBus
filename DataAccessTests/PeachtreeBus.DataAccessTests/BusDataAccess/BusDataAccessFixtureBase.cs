using System.Threading.Tasks;
using PeachtreeBus.Data;

namespace PeachtreeBus.DataAccessTests.BusDataAccess;

public abstract class BusDataAccessFixtureBase : DataAccessFixtureBase<IBusDataAccess>
{
    public override async Task Initialize()
    {
        await base.Initialize();
        BusDataAccess.Reconnect();
    }
}