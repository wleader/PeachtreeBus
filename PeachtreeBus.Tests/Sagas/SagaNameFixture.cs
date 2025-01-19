using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using PeachtreeBus.Sagas;
using PeachtreeBus.Tests.Data;

namespace PeachtreeBus.Tests.Queues;

[TestClass]
public class SagaNameFixture : DbSafeNameFixtureBase
{
    [TestMethod]
    public void Given_ForbiddenCharacters_When_New_Then_Throws()
    {
        AssertActionThrowsForDbUnsafeValues((s) => { _ = new SagaName(s); });
    }

    [TestMethod]
    public void Given_Uninitialized_When_ToString_Then_Throws()
    {
        SagaName sagaName = default!;
        Assert.ThrowsException<DbSafeNameException>(() => _ = sagaName.ToString());
    }

    [TestMethod]
    public void Given_EmptyString_When_New_Then_Throws()
    {
        Assert.ThrowsException<DbSafeNameException>(() => { _ = new SagaName(string.Empty); });
    }
}
