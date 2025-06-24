using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class DapperDataAccess_InsertSagaData_Fixture : DapperDataAccess_FixtureBase
{
    [TestMethod]
    public async Task Given_SagaDataIsNull_When_InsertSagaData_Then_Throw()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            _dataAccess.InsertSagaData(null!, TestData.DefaultSagaName));
    }

    [TestMethod]
    public async Task Given_SagaData_When_InsertSagaData_Then_TableNameIsSet()
    {
        await _dataAccess.InsertSagaData(TestData.CreateSagaData(), TestData.DefaultSagaName);
        AssertStatementContains("INTO [PBus].[DefaultSagaName_SagaData]");
    }

    [TestMethod]
    public async Task Given_SagaData_When_InsertSagaData_Then_ParametersAreSet()
    {
        var sagaData = TestData.CreateSagaData();
        await _dataAccess.InsertSagaData(sagaData, TestData.DefaultSagaName);
        AssertParameterSet("@SagaId", sagaData.SagaId);
        AssertParameterSet("@Key", sagaData.Key);
        AssertParameterSet("@Data", sagaData.Data);
        AssertParameterSet("@MetaData", sagaData.MetaData);
    }

    [TestMethod]
    public async Task Given_DapperWillThrow_When_InsertSagaData_Then_Throws()
    {
        Given_DapperThrows<Identity>();
        await Assert.ThrowsExactlyAsync<TestException>(() =>
            _dataAccess.InsertSagaData(TestData.CreateSagaData(), TestData.DefaultSagaName));
    }
}
