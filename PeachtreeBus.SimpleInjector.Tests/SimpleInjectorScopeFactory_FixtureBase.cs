using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace PeachtreeBus.SimpleInjector.Tests;

public class SimpleInjectorScopeFactory_FixtureBase
{
    protected Container _container = default!;
    protected SimpleInjectorScopeFactory _factory = default!;

    [TestInitialize]
    public void Initialize()
    {
        _container = new Container();
        _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
        _factory = new(_container);
    }
}
