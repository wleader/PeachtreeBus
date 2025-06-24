using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Serialization;
using System.Data;

namespace PeachtreeBus.Core.Tests.Data;

public abstract class SerializedTypeHandlerFixtureBase<TDataType>
{
    protected SerializedHandler<TDataType> _handler = default!;
    private readonly Mock<IDbDataParameter> _parameter = new();
    protected Mock<ISerializer> _serializer = new();

    protected TDataType _deserializeResult = default!;
    protected SerializedData _serializeResult = default!;

    [TestInitialize]
    public void Initialize()
    {
        _serializer.Reset();
        _parameter.Reset();

        _serializer.Setup(s => s.Deserialize<TDataType>(It.IsAny<SerializedData>()))
            .Returns(() => _deserializeResult);
        _serializer.Setup(s => s.Serialize(It.IsAny<TDataType>()))
            .Returns(() => _serializeResult);

        _handler = new(_serializer.Object);
    }

    protected void VerifySetValue(TDataType value, DbType expectedDbType, object? expectedValue)
    {
        _handler.SetValue(_parameter.Object, value);
        _parameter.VerifySet(p => p.DbType = expectedDbType, Times.Once);
        _parameter.VerifySet(p => p.Value = expectedValue, Times.Once);
    }
}
