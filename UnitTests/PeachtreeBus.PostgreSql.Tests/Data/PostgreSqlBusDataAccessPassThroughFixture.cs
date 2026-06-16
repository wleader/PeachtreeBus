using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Data;
using PeachtreeBus.Data;

namespace PeachtreeBus.PostgreSql.Tests.Data;

[TestClass]
public class PostgreSqlBusDataAccessPassThroughFixture 
    : BusDataAccessPassthroughFixtureBase<PostgreSqlBusDataAccess>
{
    protected override PostgreSqlBusDataAccess CreateDataAccess() =>
        new(
            Log.Object,
            DapperMethods.Object,
            BusConfiguration.Object,
            SharedDatabase.Object,
            BreakerProvider);
}