using Dapper;
using System;
using System.Data;

namespace PeachtreeBus.Data
{
    /// <summary>
    /// An SQL type handler for DataTime.
    /// Ensures that DateTimes are always persisted and read as UTC.
    /// </summary>
    public class DateTimeHandler : SqlMapper.TypeHandler<DateTime>
    {
        private static bool _typeHandlerAdded = false;
        private static readonly object _lock = new();

        public static void AddTypeHandler()
        {
            lock (_lock)
            {
                if (_typeHandlerAdded) return;
                SqlMapper.AddTypeHandler(new DateTimeHandler());
                _typeHandlerAdded = true;
            }
        }

        public override void SetValue(IDbDataParameter parameter, DateTime value)
        {
            parameter.Value = value;
        }

        public override DateTime Parse(object value)
        {
            return DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc);
        }
    }
}
