using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Sagas;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Tests.Sagas;

[TestClass]
public class CategoryFixture
{
    [TestMethod]
    [DataRow((string)null!)]
    [DataRow("")]
    public void Given_String_When_New_Then_Throws(string value)
    {
        Assert.ThrowsException<CategoryException>(() => _ = new Category(value));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(SagaKey.MaxLength + 1)]
    public void Given_StringOfLength_When_New_Then_Throws(int length)
    {
        Assert.ThrowsException<CategoryException>(() =>
            _ = new Category(new string('x', length)));
    }

    [TestMethod]
    public void Given_StringOfMaxLength_When_New_Then_Result()
    {
        var expected = new string('U', Category.MaxLength);
        var actual = new Category(expected);
        Assert.AreEqual(expected, actual.Value);
    }

    [TestMethod]
    public void Given_Category_When_ToString_Then_Result()
    {
        var expected = "FooBar";
        var actual = new Category(expected);
        Assert.AreEqual(expected, actual.ToString());
        Assert.AreEqual(expected, actual.Value);
    }

    [TestMethod]
    public void Given_Uninitialized_When_ToString_Then_Throws()
    {
        Assert.ThrowsException<CategoryException>(() =>
        _ = ((Category)default).ToString());
    }

    [TestMethod]
    public void Given_Uninitialized_When_GetValue_Then_Throws()
    {
        Assert.ThrowsException<CategoryException>(() =>
        _ = ((Category)default).Value);
    }
}
