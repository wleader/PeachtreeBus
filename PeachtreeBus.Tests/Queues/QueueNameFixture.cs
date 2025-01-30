using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using PeachtreeBus.Tests.Data;

namespace PeachtreeBus.Tests.Queues;

[TestClass]
public class QueueNameFixture : DbSafeNameFixtureBase
{
    private QueueName CreateQueueName(string value) => new(value);

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
        Assert.ThrowsException<DbSafeNameException>(() =>
            _ = ((QueueName)default).ToString());
    }


    [TestMethod]
    public void Given_Uninitialized_When_GetValue_Then_Throws()
    {
        Assert.ThrowsException<DbSafeNameException>(() =>
            _ = ((QueueName)default).Value);
    }
}
