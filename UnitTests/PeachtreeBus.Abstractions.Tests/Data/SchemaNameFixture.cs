using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Abstractions.Tests;
using PeachtreeBus.Data;
using PeachtreeBus.Exceptions;

namespace PeachtreeBus.Abstractions.Tests.Data;

[TestClass]
public class SchemaNameFixture
{
    private SchemaName Create(string value) => new(value);

    [TestMethod]
    public void Given_UnsafeValues_When_New_Then_Throws()
    {
        TestHelpers.AssertFunctionThrowsForDbUnsafeValues(Create);
    }

    [TestMethod]
    public void Given_Uninitialized_When_ToString_Then_Throws()
    {
        var thrown = Assert.ThrowsException<NotInitializedException>(() =>
            _ = ((SchemaName)default).ToString());
        Assert.AreEqual(typeof(SchemaName), thrown.Type);
    }

    [TestMethod]
    [DataRow("SchemaName")]
    [DataRow("Schema_Name")]
    public void Given_AllowedValue_When_New_Then_Result(string value)
    {
        Assert.AreEqual(value, Create(value).Value);
    }

    [TestMethod]
    public void Given_Uninitialized_When_GetValue_Then_Throws()
    {
        var thrown = Assert.ThrowsException<NotInitializedException>(() =>
            _ = ((SchemaName)default).Value);
        Assert.AreEqual(typeof(SchemaName), thrown.Type);
    }

}
