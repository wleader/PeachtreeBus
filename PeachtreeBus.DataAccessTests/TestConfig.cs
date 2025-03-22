using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;

namespace PeachtreeBus.DataAccessTests
{
    public abstract class TestConfig
    {
        public static string DbConnectionString { get; set; } = default!;

        public static readonly SchemaName DefaultSchema = new("PeachtreeBus");
        private const string QueueName = "QueueName";
        protected static readonly QueueName DefaultQueue = new(QueueName);
        protected static readonly TableName QueuePending = new(QueueName + "_Pending");
        protected static readonly TableName QueueCompleted = new(QueueName + "_Completed");
        protected static readonly TableName QueueFailed = new(QueueName + "_Failed");
        private const string SagaName = "SagaName";
        protected static readonly SagaName DefaultSagaName = new(SagaName);
        protected static readonly TableName SagaData = new(SagaName + "_SagaData");
        protected static readonly TableName Subscriptions = new("Subscriptions");
        protected static readonly TableName SubscribedPending = new("Subscribed_Pending");
        protected static readonly TableName SubscribedFailed = new("Subscribed_Failed");
        protected static readonly TableName SubscribedCompleted = new("Subscribed_Completed");
    }
}
