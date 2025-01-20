using PeachtreeBus.Errors;
using SimpleInjector;

namespace PeachtreeBus.SimpleInjector;

public partial class SimpleInjectorExtensions
{
    public static Container UsePeachtreeBusQueueRetryStrategy<TStrategy>(
        this Container container,
        Lifestyle lifestyle)
    where TStrategy : IQueueRetryStrategy
    {
        container.Register(
            typeof(IQueueRetryStrategy),
            typeof(TStrategy),
            lifestyle);
        return container;
    }

    public static Container UsePeachtreeBusSubscribedRetryStrategy<TStrategy>(
        this Container container,
        Lifestyle lifestyle)
        where TStrategy : ISubscribedRetryStrategy
    {
        container.Register(
            typeof(ISubscribedRetryStrategy),
            typeof(TStrategy),
            lifestyle);
        return container;
    }

    public static Container UsePeachtreeBusDefaultRetryStrategy(this Container container)
    {
        return container
            .UsePeachtreeBusQueueRetryStrategy<DefaultQueueRetryStrategy>(Lifestyle.Singleton)
            .UsePeachtreeBusSubscribedRetryStrategy<DefaultSubscribedRetryStrategy>(Lifestyle.Singleton);
    }
}
