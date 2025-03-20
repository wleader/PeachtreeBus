using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System.Diagnostics.CodeAnalysis;

namespace PeachtreeBus.SimpleInjector
{
    /// <summary>
    /// There are some interfaces that PeachtreeBus needs that are otherwise
    /// not used via constructor injection. This class deliberately uses them via constructor injection
    /// this allows simple injector to detect if they are not missing when verifying the container.
    /// otherwise this class is useless.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class VerifyBaseRequirements(
        IWrappedScopeFactory wrappedScopeFactory,
        IShareObjectsBetweenScopes shareObjectsBetweenScopes,
        ISqlConnectionFactory sqlConnectionFactory)
    {
        public IWrappedScopeFactory WrappedScopeFactory { get; } = wrappedScopeFactory;
        public IShareObjectsBetweenScopes ShareObjectsBetweenScopes { get; } = shareObjectsBetweenScopes;
        public ISqlConnectionFactory SqlConnectionFactory { get; } = sqlConnectionFactory;
    }

    [ExcludeFromCodeCoverage]
    public class VerifyQueueRequirements(
         IHandleFailedQueueMessages handleFailedQueueMessages)
    {
        public IHandleFailedQueueMessages HandleFailedQueueMesssages { get; } = handleFailedQueueMessages;
    }


    [ExcludeFromCodeCoverage]
    public class VerifiySubscriptionsRequirements(
         IHandleFailedSubscribedMessages handleFailedSubscribedMessages)
    {
        public IHandleFailedSubscribedMessages HandleFailedSubscribedMessages { get; } = handleFailedSubscribedMessages;
    }
}
