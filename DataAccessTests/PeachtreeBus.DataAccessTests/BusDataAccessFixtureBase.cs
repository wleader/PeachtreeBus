using System.Threading.Tasks;
using PeachtreeBus.Data;

namespace PeachtreeBus.DataAccessTests;

public abstract class BusDataAccessFixtureBase : DataAccessFixtureBase<IBusDataAccess>
{
    public override async Task Initialize()
    {
        await base.Initialize();
        BusDataAccess.Reconnect();
    }
}