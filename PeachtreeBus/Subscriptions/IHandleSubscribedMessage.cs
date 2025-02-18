using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions
{
    /// <summary>
    /// Defines an interface for classes that will handle a message of a given type.
    /// </summary>
    /// <typeparam name="T">A message class</typeparam>
    public interface IHandleSubscribedMessage<T> where T : ISubscribedMessage
    {
        Task Handle(ISubscribedContext context, T message);
    }
}
