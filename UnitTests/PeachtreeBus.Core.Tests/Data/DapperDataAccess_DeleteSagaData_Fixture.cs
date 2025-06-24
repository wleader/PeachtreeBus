using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class DapperDataAccess_DeleteSagaData_Fixture : DapperDataAccess_FixtureBase
{
    [TestMethod]
    public async Task Given_SagaData_When_DeleteSagaData_Then_TableNameIsSet()
    {
        await _dataAccess.DeleteSagaData(TestData.DefaultSagaName, TestData.DefaultSagaKey);
        AssertStatementContains("FROM [PBus].[DefaultSagaName_SagaData]");
    }

    [TestMethod]
    public async Task Given_SagaData_When_DeleteSagaData_Then_ParametersAreSet()
    {
        await _dataAccess.DeleteSagaData(TestData.DefaultSagaName, TestData.DefaultSagaKey);
        AssertParameterSet("@Key", TestData.DefaultSagaKey);
    }

    [TestMethod]
    public async Task Given_DapperWillThrow_When_DeleteSagaData_Then_Throws()
    {
        Given_DapperThrows<Identity>();
        await Assert.ThrowsExactlyAsync<TestException>(() =>
            _dataAccess.DeleteSagaData(TestData.DefaultSagaName, TestData.DefaultSagaKey));
    }
}
