using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class DapperDataAccess_EstimateQueuePending_Fixture : DapperDataAccess_FixtureBase
{
    [TestMethod]
    public async Task Given_QueueName_When_EstimateQueuePending_Then_TableNameIsSet()
    {
        await _dataAccess.EstimateQueuePending(TestData.DefaultQueueName);
        AssertStatementContains("FROM [PBus].[DefaultQueueName_Pending]");
    }

    [TestMethod]
    public async Task Given_DapperWillThrow_When_EstimateQueuePending_Then_Throws()
    {
        Given_DapperThrows<long>();
        await Assert.ThrowsExactlyAsync<TestException>(() =>
            _dataAccess.EstimateQueuePending(TestData.DefaultQueueName));
    }
}
