using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace PeachtreeBus.DatabaseSharing.Tests;

[TestClass]
public class SqlConnectionFactoryFixture
{
    private SqlConnectionFactory _factory = default!;
    private readonly Mock<IProvideDbConnectionString> _provideConnectionString = new();
    private const string ConnectionString = "Server=(local)";

    [TestInitialize]
    public void Initialize()
    {
        _provideConnectionString.Reset();
        _provideConnectionString.Setup(p => p.GetDbConnectionString())
            .Returns(() => ConnectionString);

        _factory = new(_provideConnectionString.Object);
    }

    [TestMethod]
    public void When_GetConnection_Then_ConnectionStringIsSet()
    {
        var actual = _factory.GetConnection();
        Assert.IsNotNull(actual);
        Assert.AreEqual(ConnectionString, actual.Connection.ConnectionString);
    }
}
