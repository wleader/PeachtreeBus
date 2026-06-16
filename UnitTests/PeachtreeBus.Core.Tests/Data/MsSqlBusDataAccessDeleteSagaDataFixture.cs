using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class MsSqlBusDataAccessDeleteSagaDataFixture : MsSqlBusDataAccessFixtureBase
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
}
