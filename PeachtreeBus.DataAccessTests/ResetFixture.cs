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
        private Mock<IDbSchemaConfiguration> _schemaConfiguration = default!;
        private Mock<ILogger<DapperDataAccess>> _logger = default!;

        [TestInitialize]
        public void Initialize()
        {
            _sharedDb = new();
            _schemaConfiguration = new();
            _logger = new();
            _dataAccess = new DapperDataAccess(_sharedDb.Object, _schemaConfiguration.Object, _logger.Object);
        }

        [TestMethod]
        public void Reset_CallsReconnect()
        {
            _dataAccess.Reset();
            _sharedDb.Verify(sdb => sdb.Reconnect(), Times.Once());
        }
    }
}
