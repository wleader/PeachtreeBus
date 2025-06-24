using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Queues;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class DapperDataAccess_UpdateSubscribedMessage_Fixture : DapperDataAccess_FixtureBase
{
    [TestMethod]
    public async Task Given_SubscribedData_When_UpdateMessage_Then_TableNameIsSet()
    {
        await _dataAccess.UpdateMessage(TestData.CreateSubscribedData());
        AssertStatementContains("UPDATE [PBus].[Subscribed_Pending]");
    }

    [TestMethod]
    public async Task Given_ubscribedDataNull_When_UpdateMessage_Then_Thrwows()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _dataAccess.UpdateMessage(null!));
    }

    [TestMethod]
    public async Task Given_ubscribedData_When_UpdateMessage_Then_ParametersAreSet()
    {
        var message = TestData.CreateSubscribedData();
        await _dataAccess.UpdateMessage(message);
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
            _dataAccess.UpdateMessage(TestData.CreateSubscribedData()));
    }
}
