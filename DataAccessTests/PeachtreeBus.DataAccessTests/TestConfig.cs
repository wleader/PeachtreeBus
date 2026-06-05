using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;

namespace PeachtreeBus.DataAccessTests;

public interface ITestConfig
{
    SchemaName DefaultSchema { get; }
    QueueName DefaultQueue { get; }
    TableName QueuePending { get; }
    TableName QueueCompleted { get; }
    TableName QueueFailed { get; }
    SagaName DefaultSagaName { get; } 
    TableName SagaData { get; }
    TableName Subscriptions { get; }
    TableName SubscribedPending { get; } 
    TableName SubscribedFailed { get; }
    TableName SubscribedCompleted { get; } 
}

public sealed class TestConfig : ITestConfig
{
    public SchemaName DefaultSchema { get; } = new ("PeachtreeBus");
    private const string QueueName = "QueueName";
    public QueueName DefaultQueue { get; } = new(QueueName);
    public TableName QueuePending { get; } = new(QueueName + "_Pending");
    public TableName QueueCompleted { get; } = new(QueueName + "_Completed");
    public TableName QueueFailed { get; } = new(QueueName + "_Failed");
    private const string SagaName = "SagaName";
    public SagaName DefaultSagaName { get; } = new(SagaName);
    public TableName SagaData { get; } = new(SagaName + "_SagaData");
    public TableName Subscriptions { get; } = new("Subscriptions");
    public TableName SubscribedPending { get; } = new("Subscribed_Pending");
    public TableName SubscribedFailed { get; } = new("Subscribed_Failed");
    public TableName SubscribedCompleted { get; } = new("Subscribed_Completed");
}
