using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests.Data;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;

namespace PeachtreeBus.MsSql.Tests.Data;

[TestClass]
public sealed class MsSqlBusDataAccessNullArgumentsFixture
    : BusDataAccessNullArgumentsFixtureBase<MsSqlBusDataAccess>
{
    private readonly Mock<ISqlSharedDatabase> _sharedDb = new();
    private readonly Mock<IBusConfiguration> _configuration = new();
    private readonly Mock<ILogger<MsSqlBusDataAccess>> _log = new();
    private readonly Mock<IDapperMethods> _dapper = new();
    private readonly FakeBreakerProvider _breakerProvider = new();

    protected override MsSqlBusDataAccess CreateDataAccess()
    {
        return new(
            _sharedDb.Object,
            _configuration.Object,
            _log.Object,
            _dapper.Object,
            _breakerProvider);
    }
}