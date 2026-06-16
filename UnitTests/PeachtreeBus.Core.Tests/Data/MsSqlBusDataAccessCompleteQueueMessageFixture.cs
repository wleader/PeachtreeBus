using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class MsSqlBusDataAccessCompleteQueueMessageFixture : MsSqlBusDataAccessFixtureBase
{
    [TestMethod]
    public async Task Given_QueueData_When_CompleteMessage_Then_TableNameIsSet()
    {
        await _dataAccess.CompleteMessage(TestData.CreateQueueData(), TestData.DefaultQueueName);
        AssertStatementContains("INTO [PBus].[DefaultQueueName_Completed]");
        AssertStatementContains("FROM [PBus].[DefaultQueueName_Pending]");
    }

    [TestMethod]
    public async Task Given_QueueData_When_CompleteMessage_Then_ParametersAreSet()
    {
        var message = TestData.CreateQueueData();
        await _dataAccess.CompleteMessage(message, TestData.DefaultQueueName);
        AssertParameterSet("@Id", message.Id);
    }
}
