using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using PeachtreeBus.Tests.Data;

namespace PeachtreeBus.Tests.Queues;

[TestClass]
public class QueueNameFixture : DbSafeNameFixtureBase
{
    private QueueName CreateQueueName(string value) => new QueueName(value);

    [TestMethod]
    public void Given_AllowedValue_When_New_Then_Result()
    {
        Assert.AreEqual("QueueName", CreateQueueName("QueueName").Value);
    }

    [TestMethod]
    public void Given_ForbiddenCharacters_When_New_Then_Throws()
    {
        AssertFunctionThrowsForDbUnsafeValues(CreateQueueName);
    }

    [TestMethod]
    public void Given_Uninitialized_When_ToString_Then_Throws()
    {
        QueueName queueName = default!;
        Assert.ThrowsException<DbSafeNameException>(() => _ = queueName.ToString());
    }
}
