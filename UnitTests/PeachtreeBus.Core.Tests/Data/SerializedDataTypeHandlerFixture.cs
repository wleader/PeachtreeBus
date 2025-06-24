using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using System.Data;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class SerializedDataTypeHandlerFixture : TypeHandlerFixtureBase<SerializedData, SerializedDataHandler>
{
    [TestMethod]
    [DataRow("{}")]
    [DataRow("SERIALIZEDDATA")]
    [DataRow("{\"foo\":\"bar\"}")]
    public void Given_Value_When_SetValue_Then_ParameterIsSetup(string value)
    {
        VerifySetValue(new SerializedData(value), DbType.String, value);
    }

    [TestMethod]
    [DataRow("{}")]
    [DataRow("SERIALIZEDDATA")]
    [DataRow("{\"foo\":\"bar\"}")]
    public void Given_Value_When_Parse_Then_Result(string value)
    {
        VerifyParse(value, new SerializedData(value));
    }
}
