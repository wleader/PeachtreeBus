using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using System.Text.Json;

namespace PeachtreeBus.Tests;

[TestClass]
public class BusTypesJsonSerializationFixture
{
    public class Container
    {
        public Identity Identity { get; set; }
        public SerializedData SerializedData { get; set; }
    }


    [TestMethod]
    public void Given_ClassWithBusTypes_When_RoundTripJson_Then_ValuesAreCorrect()
    {
        var expected = new Container()
        {
            Identity = new(10),
            SerializedData = new SerializedData("Foo"),
        };

        var serialized = JsonSerializer.Serialize(expected);
        var actual = JsonSerializer.Deserialize<Container>(serialized);

        Assert.IsNotNull(actual);
        Assert.AreEqual(expected.Identity, actual.Identity);
        Assert.AreEqual(expected.SerializedData, actual.SerializedData);
    }
}
