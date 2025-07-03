using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PeachtreeBus.Testing.Tests;

[TestClass]
public class UnintializedObjectsFixture
{
    [TestMethod]
    public void When_Create_Then_NotNull()
    {
        var actual = UninitializedObjects.Create<SqlConnection>();
        Assert.IsNotNull(actual);
    }

    [TestMethod]
    public void Given_Create_When_Create_Then_NotSame()
    {
        var notExpected = UninitializedObjects.Create<SqlConnection>();
        var actual = UninitializedObjects.Create<SqlConnection>();
        Assert.AreNotSame(notExpected, actual);
    }
}
