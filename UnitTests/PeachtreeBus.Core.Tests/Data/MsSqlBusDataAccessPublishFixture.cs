using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class MsSqlBusDataAccessPublishFixture : MsSqlBusDataAccessFixtureBase
{
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
}
