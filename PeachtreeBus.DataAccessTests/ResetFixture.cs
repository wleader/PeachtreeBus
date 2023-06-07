using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    [TestClass]
    public class ResetFixture
    {
        private DapperDataAccess _dataAccess;
        private Mock<ISharedDatabase> _sharedDb;
        private Mock<IDbSchemaConfiguration> _schemaConfiguration;
        private Mock<ILogger<DapperDataAccess>> _logger;

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
