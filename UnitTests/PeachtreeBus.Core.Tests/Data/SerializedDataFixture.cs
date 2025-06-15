using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;

namespace PeachtreeBus.Core.Tests.Data;

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
        Assert.ThrowsException<SerializedDataException>(() =>
            _ = ((SerializedData)default).ToString());
    }

    [TestMethod]
    [DataRow((string)null!)]
    [DataRow("")]
    public void Given_BadData_When_New_Then_Throws(string poison)
    {
        Assert.ThrowsException<SerializedDataException>(() =>
        _ = new SerializedData(poison));
    }

    [TestMethod]
    public void Given_Uninitialized_When_GetValue_Then_Throws()
    {
        Assert.ThrowsException<SerializedDataException>(() =>
            _ = ((SerializedData)default).Value);
    }
}
