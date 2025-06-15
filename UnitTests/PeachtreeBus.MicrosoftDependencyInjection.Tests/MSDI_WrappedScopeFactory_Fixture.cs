using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace PeachtreeBus.MicrosoftDependencyInjection.Tests;

[TestClass]
public class MSDI_WrappedScopeFactory_Fixture
{
    protected IServiceCollection _container = default!;

    [TestInitialize]
    public void Initialize()
    {
        _container = new ServiceCollection();
    }

    [TestMethod]
    public void Given_RegisteredIWrappedScopeIsNotSimpleInjectorWrappedScope_When_Create_Then_Throws()
    {
        var wrapped = new Mock<IWrappedScope>();
        _container.AddSingleton(typeof(IWrappedScope), new Mock<IWrappedScope>().Object);
        var _factory = new MSDIWrappedScopeFactory(_container.BuildServiceProvider());
        Assert.ThrowsExactly<MSDIWrappedScopeFactoryException>(() => _ = _factory.Create());
    }

    [TestMethod]
    public void Given_RegisteredIWrappedScopeIsSimpleInjectorWrappedScope_When_Create_Then_Result()
    {
        _container.AddSingleton(typeof(IWrappedScope), new MSDIWrappedScope());
        var _factory = new MSDIWrappedScopeFactory(_container.BuildServiceProvider());
        var actual = _factory.Create();
        Assert.AreEqual(typeof(MSDIWrappedScope), actual.GetType());
    }
}
