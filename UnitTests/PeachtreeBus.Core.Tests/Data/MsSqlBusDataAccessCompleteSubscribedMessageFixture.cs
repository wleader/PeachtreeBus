using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class MsSqlBusDataAccessCompleteSubscribedMessageFixture : MsSqlBusDataAccessFixtureBase
{
    [TestMethod]
    public async Task Given_SubscribedData_When_UpdateMessage_Then_TableNameIsSet()
    {
        await _dataAccess.CompleteMessage(TestData.CreateSubscribedData());
        AssertStatementContains("INTO [PBus].[Subscribed_Completed]");
        AssertStatementContains("FROM [PBus].[Subscribed_Pending]");
    }

    [TestMethod]
    public async Task Given_SubscribedData_When_UpdateMessage_Then_ParametersAreSet()
    {
        var message = TestData.CreateSubscribedData();
        await _dataAccess.CompleteMessage(message);
        AssertParameterSet("@Id", message.Id);
    }

}
