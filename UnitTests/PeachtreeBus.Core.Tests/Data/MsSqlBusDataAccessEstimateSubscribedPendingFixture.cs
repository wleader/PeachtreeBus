using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class MsSqlBusDataAccessEstimateSubscribedPendingFixture : MsSqlBusDataAccessFixtureBase
{
    [TestMethod]
    public async Task Given_SubscriberId_When_EstimateSubscribedPending_Then_TableNameIsSet()
    {
        await _dataAccess.EstimateSubscribedPending(TestData.DefaultSubscriberId);
        AssertStatementContains("FROM [PBus].[Subscribed_Pending]");
    }

    [TestMethod]
    public async Task Given_SubscriberId_When_EstimateSubscribedPending_Then_ParametersAreSet()
    {
        await _dataAccess.EstimateSubscribedPending(TestData.DefaultSubscriberId);
        AssertParameterSet("@SubscriberId", TestData.DefaultSubscriberId);
    }
}
