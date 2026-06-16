using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class MsSqlBusDataAccessCleanQueueCompletedFixture : MsSqlBusDataAccessFixtureBase
{
    [TestMethod]
    public async Task Given_Parameters_When_CleanQueueCompleted_Then_TableNameIsSet()
    {
        await _dataAccess.CleanQueueCompleted(TestData.DefaultQueueName, TestData.Now, 10);
        AssertStatementContains("FROM [PBus].[DefaultQueueName_Completed]");
    }

    [TestMethod]
    public async Task Given_Parameters_When_CleanQueueCompleted_Then_ParametersAreSet()
    {
        await _dataAccess.CleanQueueCompleted(TestData.DefaultQueueName, TestData.Now, 10);
        AssertParameterSet("@MaxCount", 10);
        AssertParameterSet("@OlderThan", TestData.Now);
    }
}
