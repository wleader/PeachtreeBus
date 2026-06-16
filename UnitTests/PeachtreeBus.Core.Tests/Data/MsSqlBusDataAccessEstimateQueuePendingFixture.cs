using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class MsSqlBusDataAccessEstimateQueuePendingFixture : MsSqlBusDataAccessFixtureBase
{
    [TestMethod]
    public async Task Given_QueueName_When_EstimateQueuePending_Then_TableNameIsSet()
    {
        await _dataAccess.EstimateQueuePending(TestData.DefaultQueueName);
        AssertStatementContains("FROM [PBus].[DefaultQueueName_Pending]");
    }
}
