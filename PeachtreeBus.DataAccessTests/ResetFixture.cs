using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Tests.Fakes;

namespace PeachtreeBus.DataAccessTests
{
    [TestClass]
    public class ResetFixture
    {
        private DapperDataAccess _dataAccess = default!;
        private Mock<ISharedDatabase> _sharedDb = default!;
        private Mock<IBusConfiguration> _configuration = default!;
        private Mock<ILogger<DapperDataAccess>> _logger = default!;

        [TestInitialize]
        public void Initialize()
        {
            _sharedDb = new();
            _configuration = new();
            _logger = new();
            _dataAccess = new DapperDataAccess(
                _sharedDb.Object,
                _configuration.Object,
                _logger.Object,
                FakeClock.Instance,
                TestDapperTypesHandler.Instance);
        }

        [TestMethod]
        public void Reset_CallsReconnect()
        {
            _dataAccess.Reconnect();
            _sharedDb.Verify(sdb => sdb.Reconnect(), Times.Once());
        }
    }
}
