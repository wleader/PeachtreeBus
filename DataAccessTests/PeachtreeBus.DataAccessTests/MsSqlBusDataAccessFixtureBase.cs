using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests;
using PeachtreeBus.Data;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests;

public abstract class MsSqlBusDataAccessFixtureBase : FixtureBase<MsSqlBusDataAccess>
{
    protected override MsSqlBusDataAccess CreateDataAccess()
    {
        return new MsSqlBusDataAccess(
            SharedDB,
            Configuration.Object,
            MockLog.Object,
            DapperMethods,
            FakeBreakerProvider);
    }


}
