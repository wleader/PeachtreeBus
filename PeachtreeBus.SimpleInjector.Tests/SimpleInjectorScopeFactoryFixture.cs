using Moq;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace PeachtreeBus.SimpleInjector.Tests;

[TestClass]
public class SimpleInjectorScopeFactoryFixture
{
    [TestMethod]
    public void Given_RegisteredIWrappedScopeIsNotSimpleInjectorWrappedScope_When_Create_Then_Throws()
    {
        var _container = new Container();
        _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

        var wrapped = new Mock<IWrappedScope>();
        _container.RegisterInstance(wrapped.Object);

        var factory = new SimpleInjectorScopeFactory(_container);

        Assert.ThrowsException<SimpleInjectorScopeFactoryException>(() => _ = factory.Create());
    }


    [TestMethod]
    public void Given_RegisteredIWrappedScopeIsSimpleInjectorWrappedScope_When_Create_Then_Result()
    {
        var _container = new Container();
        _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
        _container.Register(typeof(IWrappedScope), typeof(SimpleInjectorScope), Lifestyle.Scoped);
        var factory = new SimpleInjectorScopeFactory(_container);
        var actual = factory.Create();
        Assert.AreEqual(typeof(SimpleInjectorScope), actual.GetType());
    }
}
