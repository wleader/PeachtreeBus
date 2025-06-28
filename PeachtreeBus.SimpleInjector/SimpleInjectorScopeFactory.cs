using PeachtreeBus.Exceptions;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace PeachtreeBus.SimpleInjector;

public class SimpleInjectorServiceProviderAccessor : ServiceProviderAccessor<Scope>;

public class SimpleInjectorScopeFactoryException : PeachtreeBusException
{
    internal SimpleInjectorScopeFactoryException(string message) : base(message) { }
}

public class SimpleInjectorScopeFactory(Container container) : IScopeFactory
{
    private readonly Container _container = container;

    public IServiceProviderAccessor Create()
    {
        var scope = AsyncScopedLifestyle.BeginScope(_container);
        var accessor = scope.GetInstance<IServiceProviderAccessor>();
        var siAccessor = (accessor as SimpleInjectorServiceProviderAccessor)
            ?? throw new SimpleInjectorScopeFactoryException(
                "IServiceProviderAccessor is not an SimpleInjectorServiceProviderAccessor. Did you replace the registration within the Container?");
        siAccessor.Initialize(scope, scope);
        return siAccessor;
    }
}
