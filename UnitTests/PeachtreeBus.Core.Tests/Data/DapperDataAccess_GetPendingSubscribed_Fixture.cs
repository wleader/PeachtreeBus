using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Subscriptions;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class DapperDataAccess_GetPendingSubscribed_Fixture : DapperDataAccess_FixtureBase
{
    [TestMethod]
    public async Task Given_SubscriberId_When_GetPendingSubscribed_Then_TableNameIsSet()
    {
        await _dataAccess.GetPendingSubscribed(TestData.DefaultSubscriberId);
        AssertStatementContains("FROM [PBus].[Subscribed_Pending]");
    }

    [TestMethod]
    public async Task Given_SubscriberId_When_GetPendingSubscribed_Then_ParametersAreSet()
    {
        await _dataAccess.GetPendingSubscribed(TestData.DefaultSubscriberId);
        AssertParameterSet("@SubscriberId", TestData.DefaultSubscriberId);
    }

    [TestMethod]
    public async Task Given_DapperWillThrow_When_GetPendingSubscribed_Then_Throws()
    {
        Given_DapperThrows<SubscribedData>();
        await Assert.ThrowsExactlyAsync<TestException>(() =>
            _dataAccess.GetPendingSubscribed(TestData.DefaultSubscriberId));
    }
}
