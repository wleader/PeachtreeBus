using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class DapperDataAccess_FailQueueMessage_Fixture : DapperDataAccess_FixtureBase
{
    [TestMethod]
    public async Task Given_QueueData_When_UpdateMessage_Then_TableNameIsSet()
    {
        await _dataAccess.FailMessage(TestData.CreateQueueData(), TestData.DefaultQueueName);
        AssertStatementContains("INTO [PBus].[DefaultQueueName_Failed]");
        AssertStatementContains("FROM [PBus].[DefaultQueueName_Pending]");
    }

    [TestMethod]
    public async Task Given_QueueDataNull_When_UpdateMessage_Then_Thrwows()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _dataAccess.FailMessage(null!, TestData.DefaultQueueName));
    }

    [TestMethod]
    public async Task Given_QueueData_When_UpdateMessage_Then_ParametersAreSet()
    {
        var message = TestData.CreateQueueData();
        await _dataAccess.FailMessage(message, TestData.DefaultQueueName);
        AssertParameterSet("@Id", message.Id);
        AssertParameterSet("@Headers", message.Headers);
    }

    [TestMethod]
    public async Task Given_DapperWillThrow_When_UpdateMessage_Then_Throws()
    {
        Given_DapperThrows<int>();
        await Assert.ThrowsAsync<TestException>(() =>
            _dataAccess.FailMessage(TestData.CreateQueueData(), TestData.DefaultQueueName));
    }
}
