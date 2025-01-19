using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;

namespace PeachtreeBus.Tests.Data;

[TestClass]
public class SchemaNameFixture : DbSafeNameFixtureBase
{
    [TestMethod]
    public void Given_UnsafeValues_When_New_Then_Throws()
    {
        AssertActionThrowsForDbUnsafeValues((s) => { _ = new SchemaName(s); });
    }

    [TestMethod]
    public void Given_Uninitialized_When_ToString_Then_Throws()
    {
        SchemaName schemaName = default;
        Assert.ThrowsException<DbSafeNameException>(() => _ = schemaName.ToString());
    }

    [TestMethod]
    public void Given_EmptyString_When_New_Then_Throws()
    {
        Assert.ThrowsException<DbSafeNameException>(() => { _ = new SchemaName(string.Empty); });
    }
}
