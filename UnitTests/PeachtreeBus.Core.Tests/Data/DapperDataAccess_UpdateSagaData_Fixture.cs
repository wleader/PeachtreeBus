using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class DapperDataAccess_UpdateSagaData_Fixture : DapperDataAccess_FixtureBase
{
    [TestMethod]
    public async Task Given_SagaDataIsNull_When_UpdateSagaData_Then_Throw()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            _dataAccess.UpdateSagaData(null!, TestData.DefaultSagaName));
    }

    [TestMethod]
    public async Task Given_SagaData_When_UpdateSagaData_Then_TableNameIsSet()
    {
        await _dataAccess.UpdateSagaData(TestData.CreateSagaData(), TestData.DefaultSagaName);
        AssertStatementContains("UPDATE [PBus].[DefaultSagaName_SagaData]");
    }

    [TestMethod]
    public async Task Given_SagaData_When_UpdateSagaData_Then_ParametersAreSet()
    {
        var sagaData = TestData.CreateSagaData();
        await _dataAccess.UpdateSagaData(sagaData, TestData.DefaultSagaName);
        AssertParameterSet("@Id", sagaData.Id);
        AssertParameterSet("@Data", sagaData.Data);
        AssertParameterSet("@MetaData", sagaData.MetaData);
    }

    [TestMethod]
    public async Task Given_DapperWillThrow_When_UpdateSagaData_Then_Throws()
    {
        Given_DapperThrows<Identity>();
        await Assert.ThrowsExactlyAsync<TestException>(() =>
            _dataAccess.UpdateSagaData(TestData.CreateSagaData(), TestData.DefaultSagaName));
    }
}
