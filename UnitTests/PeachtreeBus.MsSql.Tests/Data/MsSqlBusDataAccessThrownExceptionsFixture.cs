using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests.Data;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;

namespace PeachtreeBus.MsSql.Tests.Data;

[TestClass]
public class MsSqlBusDataAccessThrownExceptionsFixture 
    : BusDataAccessThrownExceptionsFixtureBase<MsSqlBusDataAccess>
{
    private readonly Mock<ISqlSharedDatabase> _sharedDb = new();
    private readonly Mock<ILogger<MsSqlBusDataAccess>> _log = new();
    private readonly FakeBreakerProvider _breakerProvider = new();

    protected override MsSqlBusDataAccess CreateDataAccess() =>
        new(
            _sharedDb.Object,
            BusConfiguration.Object,
            _log.Object,
            DapperMethods.Object,
            _breakerProvider);
}