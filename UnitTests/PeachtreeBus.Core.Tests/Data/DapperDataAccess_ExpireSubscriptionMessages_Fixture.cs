using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class DapperDataAccess_ExpireSubscriptionMessages_Fixture : DapperDataAccess_FixtureBase
{
    [TestMethod]
    public async Task Given_MaxCount_When_ExpireSubscriptionMessages_Then_TableNameIsSet()
    {
        await _dataAccess.ExpireSubscriptionMessages(10);
        AssertStatementContains("FROM [PBus].[Subscribed_Pending]");
        AssertStatementContains("INTO [PBus].[Subscribed_Failed]");
    }

    [TestMethod]
    public async Task Given_MaxCount_When_ExpireSubscriptionMessages_Then_ParametersAreSet()
    {
        var message = TestData.CreateSubscribedData();
        await _dataAccess.ExpireSubscriptionMessages(10);
        AssertParameterSet("@MaxCount", 10);
    }

    [TestMethod]
    public async Task Given_DapperWillThrow_When_ExpireSubscriptionMessages_Then_Throws()
    {
        Given_DapperThrows<long>();
        await Assert.ThrowsAsync<TestException>(() =>
            _dataAccess.ExpireSubscriptionMessages(10));
    }
}