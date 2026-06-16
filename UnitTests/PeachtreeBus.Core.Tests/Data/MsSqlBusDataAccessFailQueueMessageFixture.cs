using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class MsSqlBusDataAccessFailQueueMessageFixture : MsSqlBusDataAccessFixtureBase
{
    [TestMethod]
    public async Task Given_QueueData_When_UpdateMessage_Then_TableNameIsSet()
    {
        await _dataAccess.FailMessage(TestData.CreateQueueData(), TestData.DefaultQueueName);
        AssertStatementContains("INTO [PBus].[DefaultQueueName_Failed]");
        AssertStatementContains("FROM [PBus].[DefaultQueueName_Pending]");
    }

    [TestMethod]
    public async Task Given_QueueData_When_UpdateMessage_Then_ParametersAreSet()
    {
        var message = TestData.CreateQueueData();
        await _dataAccess.FailMessage(message, TestData.DefaultQueueName);
        AssertParameterSet("@Id", message.Id);
        AssertParameterSet("@Headers", message.Headers);
    }
}
