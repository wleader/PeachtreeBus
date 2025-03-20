using PeachtreeBus.Data;
using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions;

/// <summary>
/// Defines an interface for publishing a message to all registered subscribers
/// </summary>
public interface ISubscribedPublisher
{
    Task<long> Publish(
        Topic topic,
        ISubscribedMessage message,
        UtcDateTime? notBefore = null,
        int priority = 0,
        UserHeaders? userHeaders = null);
}
