using Moq;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace PeachtreeBus.SimpleInjector.Tests;

[TestClass]
public class SimpleInjectorScopeFactoryFixture
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

    [TestMethod]
    public void Given_RegisteredIWrappedScopeIsNotSimpleInjectorWrappedScope_When_Create_Then_Throws()
    {
        var accessor = new Mock<IServiceProviderAccessor>();
        _container.RegisterInstance(accessor.Object);
        Assert.ThrowsExactly<SimpleInjectorScopeFactoryException>(() => _ = _factory.Create());
    }

    [TestMethod]
    public void Given_RegisteredIWrappedScopeIsSimpleInjectorWrappedScope_When_Create_Then_Result()
    {
        _container.Register(typeof(IServiceProviderAccessor), typeof(SimpleInjectorServiceProviderAccessor), Lifestyle.Scoped);
        var actual = _factory.Create();
        Assert.AreEqual(typeof(SimpleInjectorServiceProviderAccessor), actual.GetType());
    }
}
