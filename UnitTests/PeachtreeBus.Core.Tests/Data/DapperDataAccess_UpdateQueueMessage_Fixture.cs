using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Queues;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class DapperDataAccess_UpdateQueueMessage_Fixture : DapperDataAccess_FixtureBase
{
    [TestMethod]
    public async Task Given_QueueData_When_UpdateMessage_Then_TableNameIsSet()
    {
        await _dataAccess.UpdateMessage(TestData.CreateQueueData(), TestData.DefaultQueueName);
        AssertStatementContains("UPDATE [PBus].[DefaultQueueName_Pending]");
    }

    [TestMethod]
    public async Task Given_QueueDataNull_When_UpdateMessage_Then_Thrwows()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _dataAccess.UpdateMessage(null!, TestData.DefaultQueueName));
    }

    [TestMethod]
    public async Task Given_QueueData_When_UpdateMessage_Then_ParametersAreSet()
    {
        var message = TestData.CreateQueueData();
        await _dataAccess.UpdateMessage(message, TestData.DefaultQueueName);
        AssertParameterSet("@Id", message.Id);
        AssertParameterSet("@NotBefore", message.NotBefore);
        AssertParameterSet("@Retries", message.Retries);
        AssertParameterSet("@Headers", message.Headers);
    }

    [TestMethod]
    public async Task Given_DapperWillThrow_When_UpdateMessage_Then_Throws()
    {
        Given_DapperThrows<QueueData>();
        await Assert.ThrowsAsync<TestException>(() =>
            _dataAccess.UpdateMessage(TestData.CreateQueueData(), TestData.DefaultQueueName));
    }
}
