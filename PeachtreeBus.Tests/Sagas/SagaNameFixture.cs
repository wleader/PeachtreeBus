using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using PeachtreeBus.Sagas;
using PeachtreeBus.Tests.Data;

namespace PeachtreeBus.Tests.Queues;

[TestClass]
public class SagaNameFixture : DbSafeNameFixtureBase
{
    private SagaName CreateSagaName(string value) => new(value);

    [TestMethod]
    public void Given_AllowedValue_When_New_Then_Result()
    {
        Assert.AreEqual("SagaName", CreateSagaName("SagaName").Value);
    }

    [TestMethod]
    public void Given_ForbiddenCharacters_When_New_Then_Throws()
    {
        AssertFunctionThrowsForDbUnsafeValues(CreateSagaName);
    }

    [TestMethod]
    public void Given_Uninitialized_When_ToString_Then_Throws()
    {
        SagaName sagaName = default!;
        Assert.ThrowsException<DbSafeNameException>(() => _ = sagaName.ToString());
    }
}
