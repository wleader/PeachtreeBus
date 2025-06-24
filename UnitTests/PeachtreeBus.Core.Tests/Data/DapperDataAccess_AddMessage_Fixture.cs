using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class DapperDataAccess_AddMessage_Fixture : DapperDataAccess_FixtureBase
{
    [TestMethod]
    public async Task Given_QueueDataNull_When_AddMessage_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            _dataAccess.AddMessage(null!, TestData.DefaultQueueName));
    }

    [TestMethod]
    public async Task Given_QueueData_When_AddMessage_Then_TableNameIsSet()
    {
        await _dataAccess.AddMessage(TestData.CreateQueueData(), TestData.DefaultQueueName);
        AssertStatementContains("INTO [PBus].[DefaultQueueName_Pending]");
    }

    [TestMethod]
    public async Task Given_QueueData_When_AddMessage_Then_ParametersAreSet()
    {
        var message = TestData.CreateQueueData();
        await _dataAccess.AddMessage(message, TestData.DefaultQueueName);
        AssertParameterSet("@MessageId", message.MessageId);
        AssertParameterSet("@Priority", message.Priority);
        AssertParameterSet("@NotBefore", message.NotBefore);
        AssertParameterSet("@Headers", message.Headers);
        AssertParameterSet("@Body", message.Body);
    }

    [TestMethod]
    public async Task Given_DapperWillThrow_When_AddMessage_Then_Throws()
    {
        Given_DapperThrows<Identity>();
        await Assert.ThrowsExactlyAsync<TestException>(() =>
            _dataAccess.AddMessage(TestData.CreateQueueData(), TestData.DefaultQueueName));
    }
}
