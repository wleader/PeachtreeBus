using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Sagas;
using System.Diagnostics.CodeAnalysis;

namespace PeachtreeBus.Abstractions.Tests.Sagas;

[TestClass]
public class SagaFixture
{
    private class Data;

    private class TestSaga : Saga<Data>
    {
        public override SagaName SagaName => new("TestSaga");

        [ExcludeFromCodeCoverage]
        public override void ConfigureMessageKeys(ISagaMessageMap mapper)
        {
            throw new System.NotImplementedException();
        }
    }

    [TestMethod]
    public void Given_New_Then_DataIsNotNull()
    {
        var saga = new TestSaga();
        Assert.IsNotNull(saga.Data);
    }

    [TestMethod]
    public void Given_Saga_Then_DataIsReadWrite()
    {
        var saga = new TestSaga();
        var newData = new Data();
        saga.Data = newData;
        Assert.AreSame(newData, saga.Data);
    }

    [TestMethod]
    public void Given_New_Then_HasName()
    {
        Assert.AreEqual("TestSaga", new TestSaga().SagaName.Value);
    }

    [TestMethod]
    public void Given_New_Then_SagaCompleteIsFalse()
    {
        Assert.IsFalse(new TestSaga().SagaComplete);
    }

    [TestMethod]
    public void Given_Saga_Then_SagaCompleteIsReadWrite()
    {
        var saga = new TestSaga();
        Assert.IsFalse(saga.SagaComplete);
        saga.SagaComplete = true;
        Assert.IsTrue(saga.SagaComplete);
        saga.SagaComplete = false;
        Assert.IsFalse(saga.SagaComplete);
    }
}
