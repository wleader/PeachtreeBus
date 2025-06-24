using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using System;
using System.Data;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class UniqueIdentityDataTypeHandlerFixture : TypeHandlerFixtureBase<UniqueIdentity, UniqueIdentityHandler>
{
    [TestMethod]
    [DataRow("03414020-3f78-45a4-b6c5-8fd6aed7bf14")]
    public void Given_Value_When_SetValue_Then_ParameterIsSetup(string value)
    {
        var guid = Guid.Parse(value);
        VerifySetValue(new(guid), DbType.Guid, guid);
    }

    [TestMethod]
    [DataRow("03414020-3f78-45a4-b6c5-8fd6aed7bf14")]
    [DataRow("00000000-0000-0000-0000-000000000000")]
    public void Given_Value_When_Parse_Then_Result(string value)
    {
        var guid = Guid.Parse(value);
        var actual = _handler.Parse(guid);
        Assert.AreEqual(guid, actual.Value);
    }
}