using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;

namespace PeachtreeBus.DataAccessTests
{
    [TestClass]
    public class ResetFixture
    {
        private DapperDataAccess _dataAccess = default!;
        private Mock<ISharedDatabase> _sharedDb = default!;
        private Mock<IBusConfiguration> _configuration = default!;
        private Mock<ILogger<DapperDataAccess>> _logger = default!;
        private readonly Mock<IDapperMethods> _sqlExecutor = new();

        [TestInitialize]
        public void Initialize()
        {
            _sharedDb = new();
            _configuration = new();
            _logger = new();
            _sqlExecutor.Reset();
            _dataAccess = new DapperDataAccess(
                _sharedDb.Object,
                _configuration.Object,
                _logger.Object,
                _sqlExecutor.Object);
        }

        [TestMethod]
        public void Reset_CallsReconnect()
        {
            _dataAccess.Reconnect();
            _sharedDb.Verify(sdb => sdb.Reconnect(), Times.Once());
        }
    }
}
