using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using PeachtreeBus.Subscriptions;
using System;
using System.Collections.Generic;
using System.Data;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// A set of extension methods that makes working with the DataSets 
    /// easier from the test code. (This allows us to have tests that
    /// don't rely on an ORM package.
    /// </summary>
    public static class DatasetExtensions
    {
        /// <summary>
        /// Conversts a dataset to QueueMessages
        /// </summary>
        /// <param name="dataset"></param>
        /// <returns></returns>
        public static List<QueueMessage> ToMessages(this DataSet dataset)
        {
            return dataset.ToType(ToMessage);
        }

        // Converts a dataset to SagaDatas
        public static List<SagaData> ToSagas(this DataSet dataset)
        {
            return dataset.ToType(ToSaga);
        }

        /// <summary>
        /// Converts a dataset to Subscriptions.
        /// </summary>
        /// <param name="dataSet"></param>
        /// <returns></returns>
        public static List<SubscriptionsRow> ToSubscriptions(this DataSet dataSet)
        {
            return dataSet.ToType(ToSubscriptionRow);
        }

        /// <summary>
        /// Converts a DataSet to SubscribedMessages.
        /// </summary>
        /// <param name="dataSet"></param>
        /// <returns></returns>
        public static List<SubscribedMessage> ToSubscribed(this DataSet dataSet)
        {
            return dataSet.ToType(ToSubscribedRow);
        }

        /// <summary>
        /// Converts a set of DataRows using the supplied converter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataset"></param>
        /// <param name="convert"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Converts a DataRow to a QueueMessage
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public static QueueMessage ToMessage(this DataRow row)
        {
            return new QueueMessage
            {
                Body = new((string)row["Body"]),
                Completed = row.ToDateTimeNullable("Completed"),
                Enqueued = row.ToDateTime("Enqueued"),
                Failed = row.ToDateTimeNullable("Failed"),
                Headers = new((string)row["Headers"]),
                Id = (long)row["Id"],
                MessageId = (Guid)row["MessageId"],
                NotBefore = row.ToDateTime("NotBefore"),
                Retries = (byte)row["Retries"]
            };
        }

        /// <summary>
        /// Converts a DataRow to a SagaData
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public static SagaData ToSaga(this DataRow row)
        {
            return new SagaData
            {
                Blocked = false, //table does not actually contain a blocked column.
                Data = new((string)row["Data"]),
                Id = (long)row["Id"],
                Key = (string)row["Key"],
                SagaId = (Guid)row["SagaId"]
            };
        }

        /// <summary>
        /// Converts a DataRow to a Subsription.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public static SubscriptionsRow ToSubscriptionRow(this DataRow row)
        {
            return new SubscriptionsRow
            {
                Id = (long)row["Id"],
                SubscriberId = (Guid)row["SubscriberId"],
                Category = (string)row["Category"],
                ValidUntil = row.ToDateTime("ValidUntil")
            };
        }

        /// <summary>
        /// Converts a DataRow to a SubscribedMessage.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public static SubscribedMessage ToSubscribedRow(this DataRow row)
        {
            return new SubscribedMessage
            {
                Body = new((string)row["Body"]),
                Completed = row.ToDateTimeNullable("Completed"),
                Enqueued = row.ToDateTime("Enqueued"),
                Failed = row.ToDateTimeNullable("Failed"),
                Headers = new((string)row["Headers"]),
                Id = (long)row["Id"],
                MessageId = (Guid)row["MessageId"],
                NotBefore = row.ToDateTime("NotBefore"),
                Retries = (byte)row["Retries"],
                SubscriberId = (Guid)row["SubscriberId"],
                ValidUntil = row.ToDateTime("ValidUntil")
            };
        }

        /// <summary>
        /// Converts a DataRow column to a nullable DateTime
        /// Non-Nulls are interpreted as UTC
        /// </summary>
        /// <param name="row"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static DateTime? ToDateTimeNullable(this DataRow row, string columnName)
        {
            var val = row[columnName];
            if (val is DBNull)
                return null;
            else
                return row.ToDateTime(columnName);
        }

        /// <summary>
        /// Converts a DataRow Column into a DateTime
        /// Times are always UTC.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this DataRow row, string columnName)
        {
            var val = (DateTime)row[columnName];
            return new DateTime(val.Year, val.Month, val.Day, val.Hour, val.Minute, val.Second, val.Millisecond, DateTimeKind.Utc);
        }
    }
}
