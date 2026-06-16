using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class MsSqlBusDataAccessUpdateSagaDataFixture : MsSqlBusDataAccessFixtureBase
{


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
}
