using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class DapperDataAccess_CleanSubscribedFailed_Fixture : DapperDataAccess_FixtureBase
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

    [TestMethod]
    public async Task Given_DapperWillThrow_When_CleanSubscribedFailed_Then_Throws()
    {
        Given_DapperThrows<long>();
        await Assert.ThrowsAsync<TestException>(() =>
            _dataAccess.CleanSubscribedFailed(TestData.Now, 10));
    }
}
