using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Exceptions;

namespace PeachtreeBus.Abstractions.Tests.Exceptions;

[TestClass]
public class SerializerExceptionFixture
{
    [TestMethod]
    public void When_New_Then_PropertiesAreSet()
    {
        var serializedData = "{Foo}";
        var type = typeof(SerializerExceptionFixture);
        var message = "Oops!";
        var ex = new SerializerException(serializedData, type, message);
        Assert.AreEqual(serializedData, ex.SerializedData);
        Assert.AreEqual(type, ex.Type);
        Assert.AreEqual(message, ex.Message);
    }
}
