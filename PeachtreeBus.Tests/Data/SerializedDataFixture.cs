using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;

namespace PeachtreeBus.Tests.Data;

[TestClass]
public class SerializedDataFixture
{
    [TestMethod]
    public void Given_Data_When_ToString_Then_Result()
    {
        var data = new SerializedData("FooBar");
        Assert.AreEqual("FooBar", data.ToString());
    }

    [TestMethod]
    public void Given_Unintialized_When_ToString_Then_Exception()
    {
        SerializedData data = default!;
        Assert.ThrowsException<SerializedDataException>(() =>
            _ = data.ToString());
    }

    [TestMethod]
    [DataRow((string)null!)]
    [DataRow("")]
    public void Given_BadData_When_New_Then_Throws(string poison)
    {
        Assert.ThrowsException<SerializedDataException>(() =>
        _ = new SerializedData(poison));
    }
}
