using PeachtreeBus.Model;

namespace PeachtreeBus.Data
{
    /// <summary>
    /// Defines the interface needed by the bus to interact with the
    /// database.
    /// </summary>
    public interface IBusDataAccess
    {
        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
        void CreateSavepoint(string name);
        void RollbackToSavepoint(string name);
        QueueMessage GetOneQueueMessage(int queueId);
        void Add(QueueMessage message);
        void Add(SagaData data);
        void Save();
        void ClearChangeTracker();
        long CleanQueueMessages();
        SagaData GetSagaData(string className, string key);
        void DeleteSagaData(string className, string key);
    }
}
