using System.Threading.Tasks;

namespace PeachtreeBus
{
    /// <summary>
    /// Defines an interface for classes that will handle a message of a given type.
    /// </summary>
    /// <typeparam name="T">A message class</typeparam>
    public interface IHandleMessage<T> where T : IMessage
    {
        Task Handle(MessageContext context, T message);
    }
}
