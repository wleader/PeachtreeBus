using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Data;

namespace PeachtreeBus.Core.Tests.Data;

public abstract class TypeHandlerFixtureBase<TDataType, THandler>
    where THandler : SqlMapper.TypeHandler<TDataType>, new()
{
    protected readonly THandler _handler = new();
    private readonly Mock<IDbDataParameter> _parameter = new();

    [TestInitialize]
    public void Initialize()
    {
        _parameter.Reset();
    }

    protected void VerifySetValue(TDataType value, DbType expectedDbType, object? expectedValue)
    {
        _handler.SetValue(_parameter.Object, value);
        _parameter.VerifySet(p => p.DbType = expectedDbType, Times.Once);
        _parameter.VerifySet(p => p.Value = expectedValue, Times.Once);
    }

    protected void VerifyParse(object value, TDataType expected)
    {
        Assert.AreEqual(expected, _handler.Parse(value));
    }
}
