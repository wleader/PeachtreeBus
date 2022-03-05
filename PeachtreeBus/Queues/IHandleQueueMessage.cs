using System.Threading.Tasks;

namespace PeachtreeBus.Queues
{
    /// <summary>
    /// Defines an interface for classes that will handle a message of a given type.
    /// </summary>
    /// <typeparam name="T">A message class</typeparam>
    public interface IHandleQueueMessage<T> where T : IQueueMessage
    {
        Task Handle(QueueContext context, T message);
    }
}
