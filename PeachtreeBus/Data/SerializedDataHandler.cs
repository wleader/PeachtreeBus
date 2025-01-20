using Dapper;
using System.Data;

namespace PeachtreeBus.Data
{
    internal class SerializedDataHandler : SqlMapper.TypeHandler<SerializedData>
    {
        private static bool _typeHandlerAdded = false;
        private static readonly object _lock = new();

        public static void AddTypeHandler()
        {
            lock (_lock)
            {
                if (_typeHandlerAdded) return;
                SqlMapper.AddTypeHandler(new SerializedDataHandler());
                _typeHandlerAdded = true;
            }
        }

        public override void SetValue(IDbDataParameter parameter, SerializedData value)
        {
            parameter.DbType = DbType.String;
            parameter.Value = value.Value;
        }

        public override SerializedData Parse(object value)
        {
            return new((string)value);
        }
    }
}
