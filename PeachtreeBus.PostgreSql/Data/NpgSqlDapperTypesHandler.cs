using System;
using System.Data;
using System.Globalization;
using Dapper;
using NpgsqlTypes;
using PeachtreeBus.Sagas;
using PeachtreeBus.Serialization;

namespace PeachtreeBus.Data;

public class NpgSqlDapperTypesHandler(
    ISerializer serializer)
    : IDapperTypesHandler
{
    private static bool _typeHandlersAdded = false;
    private static readonly object _lock = new();

    /// <summary>
    /// Provides Type Converters to Dapper so that Dapper can convert
    /// our custom data types to SQL data types.
    /// </summary>
    public bool Configure()
    {
        lock (_lock)
        {
            if (_typeHandlersAdded)
                return true;

            DefaultTypeMap.MatchNamesWithUnderscores = true;

            SqlMapper.AddTypeHandler(new SerializedDataHandler());
            SqlMapper.AddTypeHandler(new NpgSqlUtcDateTimeHandler());
            SqlMapper.AddTypeHandler(new SagaKeyHandler());
            SqlMapper.AddTypeHandler(new IdentityHandler());
            SqlMapper.AddTypeHandler(new UniqueIdentityHandler());
            SqlMapper.AddTypeHandler(new SubscriberIdHandler());
            SqlMapper.AddTypeHandler(new TopicHandler());
            SqlMapper.AddTypeHandler(new SerializedHandler<Headers>(serializer));
            SqlMapper.AddTypeHandler(new SerializedHandler<SagaMetaData>(serializer));
            _typeHandlersAdded = true;
        }
        return true;
    }
    
    public class NpgSqlUtcDateTimeHandler : PeachtreeBusTypeHandler<UtcDateTime>
    {
        public override void SetValue(IDbDataParameter parameter, UtcDateTime value)
        {
            parameter.Value = value.Value;
            //parameter.Value = DateTime.SpecifyKind(value.Value, DateTimeKind.Unspecified);
        }

        public override UtcDateTime Parse(object value)
        {
            return new((DateTime)value); 
            //return new(DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc));
        }
    }
}