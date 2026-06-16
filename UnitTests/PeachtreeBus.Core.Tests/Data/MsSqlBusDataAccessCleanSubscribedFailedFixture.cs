using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class MsSqlBusDataAccessCleanSubscribedFailedFixture : MsSqlBusDataAccessFixtureBase
{
    [TestMethod]
    public async Task Given_Parameters_When_CleanSubscribedFailed_Then_TableNameIsSet()
    {
        await _dataAccess.CleanSubscribedFailed(TestData.Now, 10);
        AssertStatementContains("FROM [PBus].[Subscribed_Failed]");
    }

    [TestMethod]
    public async Task Given_Parameters_When_CleanSubscribedFailed_Then_ParametersAreSet()
    {
        await _dataAccess.CleanSubscribedFailed(TestData.Now, 10);
        AssertParameterSet("@MaxCount", 10);
        AssertParameterSet("@OlderThan", TestData.Now);
    }
}
