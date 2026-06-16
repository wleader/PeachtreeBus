using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Data;
using PeachtreeBus.Data;

namespace PeachtreeBus.MsSql.Tests.Data;

[TestClass]
public sealed class MsSqlBusDataAccessNullArgumentsFixture
    : BusDataAccessNullArgumentsFixtureBase<MsSqlBusDataAccess>
{
    protected override MsSqlBusDataAccess CreateDataAccess() =>
        new(
            SharedDatabase.Object,
            BusConfiguration.Object,
            Log.Object,
            DapperMethods.Object,
            BreakerProvider);
}