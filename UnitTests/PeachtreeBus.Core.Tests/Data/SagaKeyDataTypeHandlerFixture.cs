using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using PeachtreeBus.Sagas;
using System.Data;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class SagaKeyDataTypeHandlerFixture : TypeHandlerFixtureBase<SagaKey, SagaKeyHandler>
{
    [TestMethod]
    [DataRow("1")]
    [DataRow("SAGAKEY")]
    public void Given_Value_When_SetValue_Then_ParameterIsSetup(string value)
    {
        VerifySetValue(new SagaKey(value), DbType.String, value);
    }

    [TestMethod]
    [DataRow("1")]
    [DataRow("SAGAKEY")]
    public void Given_Value_When_Parse_Then_Result(string value)
    {
        VerifyParse(value, new SagaKey(value));
    }
}
