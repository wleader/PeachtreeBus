using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PeachtreeBus.Testing.Tests;

[TestClass]
public class SqlServerTestingFixture
{
    [TestMethod]
    public void When_CreateConnection_Then_NotNull()
    {
        Assert.IsNotNull(SqlServerTesting.CreateConnection());
    }

    [TestMethod]
    public void When_CreateTransaction_Then_NotNull()
    {
        Assert.IsNotNull(SqlServerTesting.CreateTransaction());
    }
}
