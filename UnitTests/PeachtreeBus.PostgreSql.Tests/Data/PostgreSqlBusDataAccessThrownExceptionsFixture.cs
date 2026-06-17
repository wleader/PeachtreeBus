using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests.Data;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;

namespace PeachtreeBus.PostgreSql.Tests.Data;

[TestClass]
public class PostgreSqlBusDataAccessThrownExceptionsFixture 
    : BusDataAccessThrownExceptionsFixtureBase<PostgreSqlBusDataAccess>
{
    private readonly Mock<INpgSqlSharedDatabase> _sharedDb = new();
    private readonly Mock<ILogger<PostgreSqlBusDataAccess>> _log = new();
    private readonly FakeBreakerProvider _breakerProvider = new();

    protected override PostgreSqlBusDataAccess CreateDataAccess() =>
        new(
            _log.Object,
            DapperMethods.Object,
            BusConfiguration.Object,
            _sharedDb.Object,
            _breakerProvider);
}