using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Sagas;
using PeachtreeBus.Serialization;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class DapperTypeMappersFixture
{
    private static readonly Mock<ISerializer> _serializer = new();
    private DapperTypesHandler _handler = default!;

    [TestInitialize]
    public void Initialize()
    {
        _serializer.Reset();
        _handler = new DapperTypesHandler(_serializer.Object);
    }

    [TestMethod]
    public void When_Configure_Then_SqlMapperHasTypeHandlers()
    {
        _handler.Configure();
        Assert.IsTrue(SqlMapper.HasTypeHandler(typeof(SerializedData)));
        Assert.IsTrue(SqlMapper.HasTypeHandler(typeof(UtcDateTime)));
        Assert.IsTrue(SqlMapper.HasTypeHandler(typeof(SagaKey)));
        Assert.IsTrue(SqlMapper.HasTypeHandler(typeof(Identity)));
        Assert.IsTrue(SqlMapper.HasTypeHandler(typeof(UniqueIdentity)));
        Assert.IsTrue(SqlMapper.HasTypeHandler(typeof(SubscriberId)));
        Assert.IsTrue(SqlMapper.HasTypeHandler(typeof(Topic)));
        Assert.IsTrue(SqlMapper.HasTypeHandler(typeof(Headers)));
        Assert.IsTrue(SqlMapper.HasTypeHandler(typeof(SagaMetaData)));
    }

    [TestMethod]
    public void When_ConfigureTwice_Then_NoError()
    {
        _handler.Configure();
        _handler.Configure();
    }
}
