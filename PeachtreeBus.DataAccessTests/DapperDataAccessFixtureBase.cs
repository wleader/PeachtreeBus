using PeachtreeBus.Data;

namespace PeachtreeBus.DataAccessTests
{
    public abstract class DapperDataAccessFixtureBase : FixtureBase<DapperDataAccess>
    {
        protected override DapperDataAccess CreateDataAccess()
        {
            return new DapperDataAccess(SharedDB, Configuration.Object, MockLog.Object);
        }
    }
}
