using Dapper;
using Microsoft.Extensions.Logging;
using PeachtreeBus.Sagas;
using PeachtreeBus.Serialization;
using PeachtreeBus.Subscriptions;
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace PeachtreeBus.Data;

public interface IDapperTypesHandler
{
    bool Configure();
}

public class DapperTypesHandler(
    ISerializer serializer)
    : IDapperTypesHandler
{
    private static bool _typeHandlersAdded = false;
    private static readonly object _lock = new();
    private readonly ISerializer _serializer = serializer;

    /// <summary>
    /// Provides Type Converters to Dapper so that Dapper can convert
    /// our custom data types to SQL data types.
    /// </summary>
    public bool Configure()
    {
        lock (_lock)
        {
            if (_typeHandlersAdded) return true;
            SqlMapper.AddTypeHandler(new SerializedDataHandler());
            SqlMapper.AddTypeHandler(new UtcDateTimeHandler());
            SqlMapper.AddTypeHandler(new SagaKeyHandler());
            SqlMapper.AddTypeHandler(new IdentityHandler());
            SqlMapper.AddTypeHandler(new UniqueIdentityHandler());
            SqlMapper.AddTypeHandler(new SubscriberIdHandler());
            SqlMapper.AddTypeHandler(new TopicHandler());
            SqlMapper.AddTypeHandler(new SerializedHandler<Headers>(_serializer));
            SqlMapper.AddTypeHandler(new SerializedHandler<SagaMetaData>(_serializer));
            _typeHandlersAdded = true;
        }
        return true;
    }
}

public abstract class PeachtreeBusTypeHandler<T> : SqlMapper.TypeHandler<T>;

internal class SerializedDataHandler : PeachtreeBusTypeHandler<SerializedData>
{
    public override void SetValue(IDbDataParameter parameter, SerializedData value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value.Value;
    }

    public override SerializedData Parse(object value) => new((string)value);
}

/// <summary>
/// /// Ensures that DateTimes are always persisted and read as UTC.
/// </summary>
internal class UtcDateTimeHandler : PeachtreeBusTypeHandler<UtcDateTime>
{
    public override void SetValue(IDbDataParameter parameter, UtcDateTime value)
    {
        parameter.DbType = DbType.DateTime2;
        parameter.Value = value.Value;
    }

    public override UtcDateTime Parse(object value) =>
        new(DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc));
}

internal class SagaKeyHandler : PeachtreeBusTypeHandler<SagaKey>
{
    public override SagaKey Parse(object value) => new((string)value);

    public override void SetValue(IDbDataParameter parameter, SagaKey value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value.Value;
    }
}

internal class IdentityHandler : PeachtreeBusTypeHandler<Identity>
{
    public override Identity Parse(object value)
    {
        var l = (long)value;
        return l == 0 ? Identity.Undefined : new Identity(l);
    }

    public override void SetValue(IDbDataParameter parameter, Identity value)
    {
        parameter.DbType = DbType.Int64;
        parameter.Value = value.RequireValid().Value;
    }
}

internal class UniqueIdentityHandler: PeachtreeBusTypeHandler<UniqueIdentity>
{
    public override UniqueIdentity Parse(object value)
    {
        var guid = (Guid)value;
        return guid == Guid.Empty ? UniqueIdentity.Empty : new(guid);
    }

    public override void SetValue(IDbDataParameter parameter, UniqueIdentity value)
    {
        parameter.DbType = DbType.Guid;
        parameter.Value = value.RequireValid().Value;
    }
}

internal class SubscriberIdHandler: PeachtreeBusTypeHandler<SubscriberId>
{
    public override SubscriberId Parse(object value) => new((Guid)value);

    public override void SetValue(IDbDataParameter parameter, SubscriberId value)
    {
        parameter.DbType = DbType.Guid;
        parameter.Value = value.RequreValid().Value;
    }
}

internal class TopicHandler: PeachtreeBusTypeHandler<Topic>
{
    [ExcludeFromCodeCoverage] // At the moment, we don't ever read categories from the DB so this is never used.
    public override Topic Parse(object value) => new((string)value);

    public override void SetValue(IDbDataParameter parameter, Topic value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value.Value;
    }
}

public class SerializedHandler<T>(
    ISerializer serializer)
    : PeachtreeBusTypeHandler<T>
{
    private readonly ISerializer _serializer = serializer;

    public override T Parse(object value)
    {
        // because the SQL code doesn't know
        // what serializer is in use, it can't
        // provide a dummy string, but we need to not fail
        // when reading things like message data and saga data.
        var strValue = (string)value;
        return string.IsNullOrEmpty(strValue)
            ? default!
            : _serializer.Deserialize<T>(new(strValue));
    }

    public override void SetValue(IDbDataParameter parameter, T? value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = _serializer.Serialize(value).Value;
    }
}
