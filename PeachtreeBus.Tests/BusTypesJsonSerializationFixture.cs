using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using PeachtreeBus.Subscriptions;
using System;
using System.Text.Json;

namespace PeachtreeBus.Tests;

[TestClass]
public class BusTypesJsonSerializationFixture
{
    public class Container
    {
        public Identity Identity { get; set; }
        public UniqueIdentity UniqueIdentity { get; set; }
        public SerializedData SerializedData { get; set; }
        public UtcDateTime UtcDateTime { get; set; }
        public SagaKey SagaKey { get; set; }
        public SubscriberId SubscriberId { get; set; }
        public Category Category { get; set; }
        public SchemaName SchemaName { get; set; }
        public QueueName QueueName { get; set; }
        public SagaName SagaName { get; set; }
    }


    [TestMethod]
    public void Given_ClassWithBusTypes_When_RoundTripJson_Then_ValuesAreCorrect()
    {
        var expected = new Container()
        {
            Identity = new(10),
            UniqueIdentity = UniqueIdentity.New(),
            SerializedData = new SerializedData("Foo"),
            UtcDateTime = DateTime.UtcNow,
            SagaKey = new("Bar"),
            SubscriberId = SubscriberId.New(),
            Category = new("Baz"),
            SchemaName = new("Faz"),
            QueueName = new("Far"),
            SagaName = new("Boo"),
        };

        var serialized = JsonSerializer.Serialize(expected);
        var actual = JsonSerializer.Deserialize<Container>(serialized);

        Assert.IsNotNull(actual);
        Assert.AreEqual(expected.Identity, actual.Identity);
        Assert.AreEqual(expected.UniqueIdentity, actual.UniqueIdentity);
        Assert.AreEqual(expected.SerializedData, actual.SerializedData);
        Assert.AreEqual(expected.UtcDateTime, actual.UtcDateTime);
        Assert.AreEqual(expected.SagaKey, actual.SagaKey);
        Assert.AreEqual(expected.SubscriberId, actual.SubscriberId);
        Assert.AreEqual(expected.Category, actual.Category);
        Assert.AreEqual(expected.SchemaName, actual.SchemaName);
        Assert.AreEqual(expected.QueueName, actual.QueueName);
        Assert.AreEqual(expected.SagaName, actual.SagaName);
    }
}
