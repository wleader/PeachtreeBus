using Dapper;
using PeachtreeBus.Model;
using System;
using System.Data;

namespace PeachtreeBus.Data
{
    /// <summary>
    /// An SQL type handler for DataTime.
    /// Ensures that DateTimes are always persisted and read as UTC.
    /// </summary>
    public class UtcDateTimeHandler : SqlMapper.TypeHandler<UtcDateTime>
    {
        private static bool _typeHandlerAdded = false;
        private static readonly object _lock = new();

        public static void AddTypeHandler()
        {
            lock (_lock)
            {
                if (_typeHandlerAdded) return;
                SqlMapper.AddTypeHandler(new UtcDateTimeHandler());
                _typeHandlerAdded = true;
            }
        }

        public override void SetValue(IDbDataParameter parameter, UtcDateTime value)
        {
            parameter.DbType = DbType.DateTime2;
            parameter.Value = value.Value;
        }

        public override UtcDateTime Parse(object value)
        {
            return DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc);
        }
    }
}
