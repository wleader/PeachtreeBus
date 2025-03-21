using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Exceptions;
using PeachtreeBus.Queues;

namespace PeachtreeBus.Absractions.Tests.Queues;

[TestClass]
public class QueueNameFixture
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
        TestHelpers.AssertFunctionThrowsForDbUnsafeValues(CreateQueueName);
    }

    [TestMethod]
    public void Given_Uninitialized_When_ToString_Then_Throws()
    {
        var thrown = Assert.ThrowsException<NotInitializedException>(() =>
            _ = ((QueueName)default).ToString());
        Assert.AreEqual(typeof(QueueName), thrown.Type);
    }


    [TestMethod]
    public void Given_Uninitialized_When_GetValue_Then_Throws()
    {
        var thrown = Assert.ThrowsException<NotInitializedException>(() =>
            _ = ((QueueName)default).Value);
        Assert.AreEqual(typeof(QueueName), thrown.Type);
    }
}
