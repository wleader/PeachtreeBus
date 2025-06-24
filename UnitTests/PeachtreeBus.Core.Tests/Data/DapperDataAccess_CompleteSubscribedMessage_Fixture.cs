using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class DapperDataAccess_CompleteSubscribedMessage_Fixture : DapperDataAccess_FixtureBase
{
    [TestMethod]
    public async Task Given_SubscribedData_When_UpdateMessage_Then_TableNameIsSet()
    {
        await _dataAccess.CompleteMessage(TestData.CreateSubscribedData());
        AssertStatementContains("INTO [PBus].[Subscribed_Completed]");
        AssertStatementContains("FROM [PBus].[Subscribed_Pending]");
    }

    [TestMethod]
    public async Task Given_SubscribedDataNull_When_UpdateMessage_Then_Thrwows()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _dataAccess.CompleteMessage(null!));
    }

    [TestMethod]
    public async Task Given_SubscribedData_When_UpdateMessage_Then_ParametersAreSet()
    {
        var message = TestData.CreateSubscribedData();
        await _dataAccess.CompleteMessage(message);
        AssertParameterSet("@Id", message.Id);
    }

    [TestMethod]
    public async Task Given_DapperWillThrow_When_UpdateMessage_Then_Throws()
    {
        Given_DapperThrows<int>();
        await Assert.ThrowsAsync<TestException>(() =>
            _dataAccess.CompleteMessage(TestData.CreateSubscribedData()));
    }
}
