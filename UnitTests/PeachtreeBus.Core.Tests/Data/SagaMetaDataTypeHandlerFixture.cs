using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Sagas;
using System.Data;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class SagaMetaDataTypeHandlerFixture : SerializedTypeHandlerFixtureBase<SagaMetaData>
{
    [TestMethod]
    public void Given_Value_When_SetValue_Then_ParameterIsSetup()
    {
        _serializeResult = new("SERIALIZEDDATA");
        var metadata = new SagaMetaData();
        VerifySetValue(metadata, DbType.String, "SERIALIZEDDATA");
        _serializer.Verify(s => s.Serialize(metadata), Times.Once);
        _serializer.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Given_Value_When_Parse_Then_Result()
    {
        _deserializeResult = new();
        Assert.AreEqual(_deserializeResult, _handler.Parse("SOURCEDATA"));
        _serializer.Verify(s => s.Deserialize<SagaMetaData>(new("SOURCEDATA")), Times.Once);
        _serializer.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Given_Empty_When_Parse_Then_ResultIsDefault()
    {
        Assert.AreEqual(default, _handler.Parse(""));
    }

    [TestMethod]
    public void Given_Null_When_Parse_Then_ResultIsDefault()
    {
        Assert.AreEqual(default, _handler.Parse(null!));
    }
}
