using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace PeachtreeBus.DataAccessTests
{
    public static class DatasetExtensions
    {
        public static List<Model.QueueMessage> ToMessages(this DataSet dataset)
        {
            return dataset.ToType(ToMessage);
        }

        public static List<Model.SagaData> ToSagas(this DataSet dataset)
        {
            return dataset.ToType(ToSaga);
        }

        private static List<T> ToType<T>(this DataSet dataset, Func<DataRow, T> convert)
        {
            var result = new List<T>();
            foreach (DataTable table in dataset.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    result.Add(convert.Invoke(row));
                }
            }
            return result;
        }

        public static Model.QueueMessage ToMessage(this DataRow row)
        {
            return new Model.QueueMessage
            {
                Body = (string)row["Body"],
                Completed = row.ToDateTimeNullable("Completed"),
                Enqueued = row.ToDateTime("Enqueued"),
                Failed = row.ToDateTimeNullable("Failed"),
                Headers = (string)row["Headers"],
                Id = (long)row["Id"],
                MessageId = (Guid)row["MessageId"],
                NotBefore = row.ToDateTime("NotBefore"),
                Retries = (byte)row["Retries"]
            };
        }

        public static Model.SagaData ToSaga(this DataRow row)
        {
            return new Model.SagaData
            {
                Blocked = false, //table does not actually contain a blocked column.
                Data = (string)row["Data"],
                Id = (long)row["Id"],
                Key = (string)row["Key"],
                SagaId = (Guid)row["SagaId"]
            };
        }

        public static DateTime? ToDateTimeNullable(this DataRow row, string columnName)
        {
            var val = row[columnName];
            if (val is DBNull)
                return null;
            else
                return row.ToDateTime(columnName);
        }

        public static DateTime ToDateTime(this DataRow row, string columnName)
        {
            var val = (DateTime)row[columnName];
            return new DateTime(val.Year, val.Month, val.Day, val.Hour, val.Minute, val.Second, val.Millisecond, DateTimeKind.Utc);
        }
    }
}
