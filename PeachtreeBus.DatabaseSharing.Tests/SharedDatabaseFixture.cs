using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace PeachtreeBus.DatabaseSharing.Tests
{
    [TestClass]
    public class SharedDatabaseFixture
    {
        private bool _disposed = false;
        private SharedDatabase _db = default!;
        
        [TestInitialize]
        public void Init()
        {
            var connectionFactory = new Mock<ISqlConnectionFactory>();
            var connection = new Mock<ISqlConnection>();

            connection.Setup(c => c.Dispose()).Callback(() => _disposed = true);

            connectionFactory.Setup(f => f.GetConnection()).Returns(connection.Object);

            _db = new SharedDatabase(connectionFactory.Object);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Given_DenyDispose_When_Dispose_Then_Disposed(bool deny)
        {
            _db.DenyDispose = deny;
            _db.Dispose();
            Assert.AreEqual(!deny, _disposed);
        }
    }
}