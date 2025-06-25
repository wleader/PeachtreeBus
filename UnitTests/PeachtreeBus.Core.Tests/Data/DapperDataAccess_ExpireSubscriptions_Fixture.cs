using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class DapperDataAccess_ExpireSubscriptions_Fixture : DapperDataAccess_FixtureBase
{
    [TestMethod]
    public async Task Given_SubscribeParams_When_Subscribe_Then_TableNameIsSet()
    {
        await _dataAccess.ExpireSubscriptions(10);
        AssertStatementContains("FROM [PBus].[Subscriptions]");
    }

    [TestMethod]
    public async Task Given_SubscribeParams_When_Subscribe_Then_ParametersAreSet()
    {
        await _dataAccess.ExpireSubscriptions(10);
        AssertParameterSet("@MaxCount", 10);
    }

    [TestMethod]
    public async Task Given_DapperWillThrow_When_Subscribe_Then_Throws()
    {
        Given_DapperThrows<long>();
        await Assert.ThrowsAsync<TestException>(() =>
            _dataAccess.ExpireSubscriptions(10));
    }
}
