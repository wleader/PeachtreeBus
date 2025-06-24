using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class DapperDataAccess_Subscribe_Fixture : DapperDataAccess_FixtureBase
{
    [TestMethod]
    public async Task Given_SubscribeParams_When_Subscribe_Then_TableNameIsSet()
    {
        await _dataAccess.Subscribe(TestData.DefaultSubscriberId, TestData.DefaultTopic, TestData.Now);
        AssertStatementContains("UPDATE [PBus].[Subscriptions]");
        AssertStatementContains("INTO [PBus].[Subscriptions]");
    }

    [TestMethod]
    public async Task Given_SubscribeParams_When_Subscribe_Then_ParametersAreSet()
    {
        await _dataAccess.Subscribe(TestData.DefaultSubscriberId, TestData.DefaultTopic, TestData.Now);
        AssertParameterSet("@SubscriberId", TestData.DefaultSubscriberId);
        AssertParameterSet("@Topic", TestData.DefaultTopic);
        AssertParameterSet("@ValidUntil", TestData.Now);
    }

    [TestMethod]
    public async Task Given_DapperWillThrow_When_Subscribe_Then_Throws()
    {
        Given_DapperThrows<int>();
        await Assert.ThrowsAsync<TestException>(() =>
            _dataAccess.Subscribe(TestData.DefaultSubscriberId, TestData.DefaultTopic, TestData.Now));
    }
}
