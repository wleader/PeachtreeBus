using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;

namespace PeachtreeBus.DataAccessTests
{
    [TestClass]
    public class ResetFixture
    {
        private MsSqlBusDataAccess _dataAccess = default!;
        private Mock<ISqlSharedDatabase> _sharedDb = default!;
        private Mock<IBusConfiguration> _configuration = default!;
        private Mock<ILogger<MsSqlBusDataAccess>> _logger = default!;
        private readonly Mock<IDapperMethods> _sqlExecutor = new();
        private readonly FakeBreakerProvider _breakerProvider = new();

        [TestInitialize]
        public void Initialize()
        {
            _sharedDb = new();
            _configuration = new();
            _logger = new();
            _sqlExecutor.Reset();

            _dataAccess = new MsSqlBusDataAccess(
                _sharedDb.Object,
                _configuration.Object,
                _logger.Object,
                _sqlExecutor.Object,
                _breakerProvider);
        }

        [TestMethod]
        public void Reset_CallsReconnect()
        {
            _dataAccess.Reconnect();
            _sharedDb.Verify(sdb => sdb.Reconnect(), Times.Once());
        }
    }
}
