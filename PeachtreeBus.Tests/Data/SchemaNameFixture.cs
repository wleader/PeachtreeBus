using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;

namespace PeachtreeBus.Tests.Data;

[TestClass]
public class SchemaNameFixture : DbSafeNameFixtureBase
{
    private SchemaName Create(string value) => new(value);

    [TestMethod]
    public void Given_UnsafeValues_When_New_Then_Throws()
    {
        AssertFunctionThrowsForDbUnsafeValues(Create);
    }

    [TestMethod]
    public void Given_Uninitialized_When_ToString_Then_Throws()
    {
        SchemaName schemaName = default;
        Assert.ThrowsException<DbSafeNameException>(() => _ = schemaName.ToString());
    }

    [TestMethod]
    public void Given_Value_When_New_Then_Result()
    {
        Assert.AreEqual("SchemaName", Create("SchemaName").Value);
    }
}
