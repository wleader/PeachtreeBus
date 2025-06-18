using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using PeachtreeBus.Sagas;

namespace PeachtreeBus.Core.Tests.Sagas;

[TestClass]
public class SagaDataFixture
{
    [TestMethod]
    public void Given_Values_When_New_Then_PropertiesAreSet()
    {
        var id = new Identity(12);
        var sagaId = UniqueIdentity.New();
        var key = new SagaKey("KEY");
        var data = new SerializedData("DATA");
        var metaData = new SagaMetaData();

        var d = new SagaData()
        {
            Id = id,
            SagaId = sagaId,
            Key = key,
            Data = data,
            Blocked = true,
            MetaData = metaData,
        };

        Assert.AreEqual(id, d.Id);
        Assert.AreEqual(sagaId, d.SagaId);
        Assert.AreEqual(key, d.Key);
        Assert.AreEqual(data, d.Data);
        Assert.AreEqual(metaData, d.MetaData);
    }
}
