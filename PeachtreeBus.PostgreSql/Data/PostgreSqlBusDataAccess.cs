using System.Threading.Tasks;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Data;

public class PostgreSqlBusDataAccess : IBusDataAccess
{
    public void BeginTransaction()
    {
        throw new System.NotImplementedException();
    }

    public void CommitTransaction()
    {
        throw new System.NotImplementedException();
    }

    public void RollbackTransaction()
    {
        throw new System.NotImplementedException();
    }

    public void CreateSavepoint(string name)
    {
        throw new System.NotImplementedException();
    }

    public void RollbackToSavepoint(string name)
    {
        throw new System.NotImplementedException();
    }

    public void Reconnect()
    {
        throw new System.NotImplementedException();
    }

    public Task<QueueData?> GetPendingQueued(QueueName queueName)
    {
        throw new System.NotImplementedException();
    }

    public Task<long> EstimateQueuePending(QueueName queueName)
    {
        throw new System.NotImplementedException();
    }

    public Task<Identity> AddMessage(QueueData message, QueueName queueName)
    {
        throw new System.NotImplementedException();
    }

    public Task CompleteMessage(QueueData message, QueueName queueName)
    {
        throw new System.NotImplementedException();
    }

    public Task FailMessage(QueueData message, QueueName queueName)
    {
        throw new System.NotImplementedException();
    }

    public Task UpdateMessage(QueueData message, QueueName queueName)
    {
        throw new System.NotImplementedException();
    }

    public Task<Identity> InsertSagaData(SagaData data, SagaName sagaName)
    {
        throw new System.NotImplementedException();
    }

    public Task UpdateSagaData(SagaData data, SagaName sagaName)
    {
        throw new System.NotImplementedException();
    }

    public Task<SagaData?> GetSagaData(SagaName sagaName, SagaKey key)
    {
        throw new System.NotImplementedException();
    }

    public Task DeleteSagaData(SagaName sagaName, SagaKey key)
    {
        throw new System.NotImplementedException();
    }

    public Task<long> ExpireSubscriptions(int maxCount)
    {
        throw new System.NotImplementedException();
    }

    public Task Subscribe(SubscriberId subscriberId, Topic topic, UtcDateTime until)
    {
        throw new System.NotImplementedException();
    }

    public Task<SubscribedData?> GetPendingSubscribed(SubscriberId subscriberId)
    {
        throw new System.NotImplementedException();
    }

    public Task<long> EstimateSubscribedPending(SubscriberId subscriberId)
    {
        throw new System.NotImplementedException();
    }

    public Task<long> Publish(SubscribedData message, Topic topic)
    {
        throw new System.NotImplementedException();
    }

    public Task CompleteMessage(SubscribedData message)
    {
        throw new System.NotImplementedException();
    }

    public Task FailMessage(SubscribedData message)
    {
        throw new System.NotImplementedException();
    }

    public Task UpdateMessage(SubscribedData message)
    {
        throw new System.NotImplementedException();
    }

    public Task<long> ExpireSubscriptionMessages(int maxCount)
    {
        throw new System.NotImplementedException();
    }

    public Task<long> CleanQueueFailed(QueueName queueName, UtcDateTime olderthan, int maxCount)
    {
        throw new System.NotImplementedException();
    }

    public Task<long> CleanQueueCompleted(QueueName queueName, UtcDateTime olderthan, int maxCount)
    {
        throw new System.NotImplementedException();
    }

    public Task<long> CleanSubscribedCompleted(UtcDateTime olderthan, int maxCount)
    {
        throw new System.NotImplementedException();
    }

    public Task<long> CleanSubscribedFailed(UtcDateTime olderthan, int maxCount)
    {
        throw new System.NotImplementedException();
    }
}