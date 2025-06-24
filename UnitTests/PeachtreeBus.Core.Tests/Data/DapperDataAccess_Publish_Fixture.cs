using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class DapperDataAccess_Publish_Fixture : DapperDataAccess_FixtureBase
{
    [TestMethod]
    public async Task Given_SubscribedDataNull_When_AddMessage_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            _dataAccess.Publish(null!, TestData.DefaultTopic));
    }

    [TestMethod]
    public async Task Given_SubscribedData_When_Publish_Then_TableNameIsSet()
    {
        await _dataAccess.Publish(TestData.CreateSubscribedData(), TestData.DefaultTopic);
        AssertStatementContains("INTO [PBus].[Subscribed_Pending]");
        AssertStatementContains("FROM [PBus].[Subscriptions]");
    }

    [TestMethod]
    public async Task Given_SubscribedData_When_Publish_Then_ParametersAreSet()
    {
        var message = TestData.CreateSubscribedData();
        await _dataAccess.Publish(message, TestData.DefaultTopic);
        AssertParameterSet("@Priority", message.Priority);
        AssertParameterSet("@ValidUntil", message.ValidUntil);
        AssertParameterSet("@NotBefore", message.NotBefore);
        AssertParameterSet("@Headers", message.Headers);
        AssertParameterSet("@Body", message.Body);
        AssertParameterSet("@Topic", TestData.DefaultTopic);
    }

    [TestMethod]
    public async Task Given_DapperWillThrow_When_AddMessage_Then_Throws()
    {
        Given_DapperThrows<long>();
        await Assert.ThrowsExactlyAsync<TestException>(() =>
            _dataAccess.Publish(TestData.CreateSubscribedData(), TestData.DefaultTopic));
    }
}
