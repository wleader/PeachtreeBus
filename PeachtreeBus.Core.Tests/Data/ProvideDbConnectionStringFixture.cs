using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class ProvideDbConnectionStringFixture
{
    [TestMethod]
    public void Given_Configuratio_When_GetDBConnectionString_Then_ConnectionStringMatches()
    {
        var busConfig = new BusConfiguration()
        {
            ConnectionString = "ConnectionString",
            Schema = new("PBUS"),
        };

        var provider = new ProvideDbConnectionString(busConfig);
        Assert.AreEqual("ConnectionString", provider.GetDbConnectionString());
    }
}
