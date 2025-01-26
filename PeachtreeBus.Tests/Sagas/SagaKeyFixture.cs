using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Sagas;

namespace PeachtreeBus.Tests.Sagas;

[TestClass]
public class SagaKeyFixture
{
    [TestMethod]
    [DataRow((string)null!)]
    [DataRow("")]
    public void Given_String_When_New_Then_Throws(string value)
    {
        Assert.ThrowsException<SagaKeyException>(() => _ = new SagaKey(value));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(SagaKey.MaxLength + 1)]
    public void Given_StringOfLength_When_New_Then_Throws(int length)
    {
        Assert.ThrowsException<SagaKeyException>(() =>
            _ = new SagaKey(new string('x', length)));
    }

    [TestMethod]
    public void Given_StringOfMaxLength_When_New_Then_Result()
    {
        var expected = new string('U', SagaKey.MaxLength);
        var actual = new SagaKey(expected);
        Assert.AreEqual(expected, actual.Value);
    }
}
