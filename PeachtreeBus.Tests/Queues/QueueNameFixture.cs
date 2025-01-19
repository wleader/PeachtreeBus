using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using PeachtreeBus.Tests.Data;

namespace PeachtreeBus.Tests.Queues;

[TestClass]
public class QueueNameFixture : DbSafeNameFixtureBase
{
    [TestMethod]
    public void Given_ForbiddenCharacters_When_New_Then_Throws()
    {
        AssertActionThrowsForDbUnsafeValues((s) => { _ = new QueueName(s); });
    }

    [TestMethod]
    public void Given_Uninitialized_When_ToString_Then_Throws()
    {
        QueueName queueName = default!;
        Assert.ThrowsException<DbSafeNameException>(() => _ = queueName.ToString());
    }

    [TestMethod]
    public void Given_EmptyString_When_New_Then_Throws()
    {
        Assert.ThrowsException<DbSafeNameException>(() => { _ = new QueueName(string.Empty); });
    }
}
