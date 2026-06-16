using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class MsSqlBusDataAccessInsertSagaDataFixture : MsSqlBusDataAccessFixtureBase
{
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
}
