using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Abstractions.Tests;

[TestClass]
public class BusConfigurationFixture
{
    private const string ConnectionString = nameof(ConnectionString);
    private readonly SchemaName Schema = new(nameof(Schema));

    [TestMethod]
    public void When_New_Then_RequiredAreCorrect()
    {
        var c = new BusConfiguration()
        {
            ConnectionString = ConnectionString,
            Schema = Schema,
        };

        Assert.AreEqual(ConnectionString, c.ConnectionString);
        Assert.AreEqual(Schema, c.Schema);
    }

    [TestMethod]
    public void When_New_Then_Defaults()
    {
        var c = new BusConfiguration()
        {
            ConnectionString = null!,
            Schema = new("Foo"),
        };

        Assert.IsNull(c.QueueConfiguration);
        Assert.IsNull(c.SubscriptionConfiguration);
        Assert.IsNotNull(c.PublishConfiguration);
        Assert.IsTrue(c.UseDefaultSerialization);
        Assert.IsTrue(c.UseStartupTasks);
    }

    [TestMethod]
    public void Givien_Inits_When_New_Then_PropertiesAreSet()
    {
        var queueConfig = new QueueConfiguration()
        {
            QueueName = new("foo")
        };
        var subsConfig = new SubscriptionConfiguration()
        {
            SubscriberId = SubscriberId.New(),
            Topics = []
        };
        var pubsConfig = new PublishConfiguration();

        var c = new BusConfiguration()
        {
            ConnectionString = null!,
            Schema = new("Foo"),
            QueueConfiguration = queueConfig,
            SubscriptionConfiguration = subsConfig,
            PublishConfiguration = pubsConfig,
            UseDefaultSerialization = false,
            UseStartupTasks = false,
        };

        Assert.AreSame(queueConfig, c.QueueConfiguration);
        Assert.AreSame(subsConfig, c.SubscriptionConfiguration);
        Assert.AreSame(pubsConfig, c.PublishConfiguration);
        Assert.IsFalse(c.UseDefaultSerialization);
        Assert.IsFalse(c.UseStartupTasks);
    }
}
