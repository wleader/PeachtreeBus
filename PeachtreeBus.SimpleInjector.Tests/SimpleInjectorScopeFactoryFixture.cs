using Moq;
using SimpleInjector;

namespace PeachtreeBus.SimpleInjector.Tests;

[TestClass]
public class SimpleInjectorScopeFactoryFixture : SimpleInjectorScopeFactory_FixtureBase
{
    [TestMethod]
    public void Given_RegisteredIWrappedScopeIsNotSimpleInjectorWrappedScope_When_Create_Then_Throws()
    {
        var wrapped = new Mock<IWrappedScope>();
        _container.RegisterInstance(wrapped.Object);
        Assert.ThrowsExactly<SimpleInjectorScopeFactoryException>(() => _ = _factory.Create());
    }

    [TestMethod]
    public void Given_RegisteredIWrappedScopeIsSimpleInjectorWrappedScope_When_Create_Then_Result()
    {
        _container.Register(typeof(IWrappedScope), typeof(SimpleInjectorScope), Lifestyle.Scoped);
        var actual = _factory.Create();
        Assert.AreEqual(typeof(SimpleInjectorScope), actual.GetType());
    }
}
