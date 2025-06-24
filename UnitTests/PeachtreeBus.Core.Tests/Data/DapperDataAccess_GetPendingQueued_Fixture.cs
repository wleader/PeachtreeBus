using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Queues;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class DapperDataAccess_GetPendingQueued_Fixture : DapperDataAccess_FixtureBase
{
    [TestMethod]
    public async Task Given_QueueName_When_GetPendingQueued_Then_TableNameIsSet()
    {
        await _dataAccess.GetPendingQueued(TestData.DefaultQueueName);
        AssertStatementContains("FROM [PBus].[DefaultQueueName_Pending]");
    }

    [TestMethod]
    public async Task Given_DapperWillThrow_When_GetPendingQueued_Then_Throws()
    {
        Given_DapperThrows<QueueData>();
        await Assert.ThrowsExactlyAsync<TestException>(() =>
            _dataAccess.GetPendingQueued(TestData.DefaultQueueName));
    }
}
