﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using PeachtreeBus.Errors;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using PeachtreeBus.Subscriptions;
using System;
using System.Text.Json;

namespace PeachtreeBus.Abstractions.Tests;

[TestClass]
public class BusTypesJsonSerializationFixture
{
    public class Container
    {
        public UniqueIdentity UniqueIdentity { get; set; }
        public UtcDateTime UtcDateTime { get; set; }
        public SagaKey SagaKey { get; set; }
        public SubscriberId SubscriberId { get; set; }
        public Topic Topic { get; set; }
        public SchemaName SchemaName { get; set; }
        public QueueName QueueName { get; set; }
        public SagaName SagaName { get; set; }
        public FailureCount FailureCount { get; set; }
    }


    [TestMethod]
    public void Given_ClassWithBusTypes_When_RoundTripJson_Then_ValuesAreCorrect()
    {
        var expected = new Container()
        {
            UniqueIdentity = UniqueIdentity.New(),
            UtcDateTime = DateTime.UtcNow,
            SagaKey = new("Bar"),
            SubscriberId = SubscriberId.New(),
            Topic = new("Baz"),
            SchemaName = new("Faz"),
            QueueName = new("Far"),
            SagaName = new("Boo"),
            FailureCount = new(5),
        };

        var serialized = JsonSerializer.Serialize(expected);
        var actual = JsonSerializer.Deserialize<Container>(serialized);

        Assert.IsNotNull(actual);
        Assert.AreEqual(expected.UniqueIdentity, actual.UniqueIdentity);
        Assert.AreEqual(expected.UtcDateTime, actual.UtcDateTime);
        Assert.AreEqual(expected.SagaKey, actual.SagaKey);
        Assert.AreEqual(expected.SubscriberId, actual.SubscriberId);
        Assert.AreEqual(expected.Topic, actual.Topic);
        Assert.AreEqual(expected.SchemaName, actual.SchemaName);
        Assert.AreEqual(expected.QueueName, actual.QueueName);
        Assert.AreEqual(expected.SagaName, actual.SagaName);
        Assert.AreEqual(expected.FailureCount, actual.FailureCount);
    }
}
