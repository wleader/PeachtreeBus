using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests.Data;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;

namespace PeachtreeBus.PostgreSql.Tests.Data;

[TestClass]
public sealed class PostGreSqlBusDataAccessNullArgumentsFixture
    : BusDataAccessNullArgumentsFixtureBase<PostgreSqlBusDataAccess>
{
    private readonly Mock<INpgSqlSharedDatabase> _sharedDb = new();
    private readonly Mock<IBusConfiguration> _configuration = new();
    private readonly Mock<ILogger<PostgreSqlBusDataAccess>> _log = new();
    private readonly Mock<IDapperMethods> _dapper = new();
    private readonly FakeBreakerProvider _breakerProvider = new();
    
    protected override PostgreSqlBusDataAccess CreateDataAccess() =>
        new(_log.Object,
            _dapper.Object,
            _configuration.Object,
            _sharedDb.Object,
            _breakerProvider);
}