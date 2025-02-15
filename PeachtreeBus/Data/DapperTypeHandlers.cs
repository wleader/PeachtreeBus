using Dapper;
using PeachtreeBus.Sagas;
using PeachtreeBus.Subscriptions;
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace PeachtreeBus.Data;

public static class DapperTypeHandlers
{
    private static bool _typeHandlersAdded = false;
    private static readonly object _lock = new();

    /// <summary>
    /// Provides Type Converters to Dapper so that Dapper can convert
    /// our custom data types to SQL data types.
    /// </summary>
    public static void AddHandlers()
    {
        lock (_lock)
        {
            if (_typeHandlersAdded) return;
            SqlMapper.AddTypeHandler(new SerializedDataHandler());
            SqlMapper.AddTypeHandler(new UtcDateTimeHandler());
            SqlMapper.AddTypeHandler(new SagaKeyHandler());
            SqlMapper.AddTypeHandler(new IdentityHandler());
            SqlMapper.AddTypeHandler(new UniqueIdentityHandler());
            SqlMapper.AddTypeHandler(new SubscriberIdHandler());
            SqlMapper.AddTypeHandler(new TopicHandler());
            _typeHandlersAdded = true;
        }
    }
}

internal class SerializedDataHandler : SqlMapper.TypeHandler<SerializedData>
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
internal class UtcDateTimeHandler : SqlMapper.TypeHandler<UtcDateTime>
{
    public override void SetValue(IDbDataParameter parameter, UtcDateTime value)
    {
        parameter.DbType = DbType.DateTime2;
        parameter.Value = value.Value;
    }

    public override UtcDateTime Parse(object value) =>
        new(DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc));
}

internal class SagaKeyHandler : SqlMapper.TypeHandler<SagaKey>
{
    public override SagaKey Parse(object value) => new((string)value);

    public override void SetValue(IDbDataParameter parameter, SagaKey value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value.Value;
    }
}

internal class IdentityHandler : SqlMapper.TypeHandler<Identity>
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

internal class UniqueIdentityHandler : SqlMapper.TypeHandler<UniqueIdentity>
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

internal class SubscriberIdHandler : SqlMapper.TypeHandler<SubscriberId>
{
    public override SubscriberId Parse(object value) => new((Guid)value);

    public override void SetValue(IDbDataParameter parameter, SubscriberId value)
    {
        parameter.DbType = DbType.Guid;
        parameter.Value = value.RequreValid().Value;
    }
}

internal class TopicHandler : SqlMapper.TypeHandler<Topic>
{
    [ExcludeFromCodeCoverage] // At the moment, we don't ever read categories from the DB so this is never used.
    public override Topic Parse(object value) => new((string)value);

    public override void SetValue(IDbDataParameter parameter, Topic value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value.Value;
    }
}