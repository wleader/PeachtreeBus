using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Sagas;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class DapperDataAccess_GetSagaData_Fixture : DapperDataAccess_FixtureBase
{
    [TestMethod]
    public async Task Given_Parameters_When_GetSagaData_Then_TableNameIsSet()
    {
        await _dataAccess.GetSagaData(TestData.DefaultSagaName, TestData.DefaultSagaKey);
        AssertStatementContains("FROM [PBus].[DefaultSagaName_SagaData]");
    }

    [TestMethod]
    public async Task Given_Parameters_When_GetSagaData_Then_ParametersAreSet()
    {
        var sagaData = TestData.CreateSagaData();
        await _dataAccess.GetSagaData(TestData.DefaultSagaName, TestData.DefaultSagaKey);
        AssertParameterSet("@Key", sagaData.Key);
    }

    [TestMethod]
    public async Task Given_DapperWillThrow_When_GetSagaData_Then_Throws()
    {
        Given_DapperThrows<SagaData>();
        await Assert.ThrowsExactlyAsync<TestException>(() =>
            _dataAccess.GetSagaData(TestData.DefaultSagaName, TestData.DefaultSagaKey));
    }
}
