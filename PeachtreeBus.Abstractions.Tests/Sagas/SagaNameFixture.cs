using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Exceptions;
using PeachtreeBus.Sagas;

namespace PeachtreeBus.Absractions.Tests.Sagas;

[TestClass]
public class SagaNameFixture
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
        TestHelpers.AssertFunctionThrowsForDbUnsafeValues(CreateSagaName);
    }

    [TestMethod]
    public void Given_Uninitialized_When_ToString_Then_Throws()
    {
        var thrown = Assert.ThrowsException<NotInitializedException>(() =>
            _ = ((SagaName)default).ToString());
        Assert.AreEqual(typeof(SagaName), thrown.Type);
    }

    [TestMethod]
    public void Given_Uninitialized_When_GetValue_Then_Throws()
    {
        var thrown = Assert.ThrowsException<NotInitializedException>(() =>
            _ = ((SagaName)default).Value);
        Assert.AreEqual(typeof(SagaName), thrown.Type);
    }
}
