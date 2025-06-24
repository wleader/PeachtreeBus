using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using System.Data;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class IdentityDataTypeHandlerFixture : TypeHandlerFixtureBase<Identity, IdentityHandler>
{
    [TestMethod]
    [DataRow((long)1)]
    [DataRow((long)123456789)]
    public void Given_Value_When_SetValue_Then_ParameterIsSetup(long value)
    {
        VerifySetValue(new Identity(value), DbType.Int64, value);
    }

    [TestMethod]
    [DataRow((long)0)]
    [DataRow((long)1)]
    [DataRow((long)123456789)]
    public void Given_Value_When_Parse_Then_Result(long value)
    {
        var actual = _handler.Parse(value);
        Assert.AreEqual(value, actual.Value);
    }
}