using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace PeachtreeBus.MicrosoftDependencyInjection.Tests;

[TestClass]
public class MSDI_ScopeFactory_Fixture
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
        var wrapped = new Mock<IServiceProviderAccessor>();
        _container.AddSingleton(typeof(IServiceProviderAccessor), wrapped.Object);
        var _factory = new MSDIScopeFactory(_container.BuildServiceProvider());
        Assert.ThrowsExactly<MSDIScopeFactoryException>(() => _ = _factory.Create());
    }

    [TestMethod]
    public void Given_RegisteredAccessoreIsMSDIAccessor_When_Create_Then_Result()
    {
        _container.AddSingleton(typeof(IServiceProviderAccessor), new MSDIServiceProviderAccessor());
        var _factory = new MSDIScopeFactory(_container.BuildServiceProvider());
        var actual = _factory.Create();
        Assert.AreEqual(typeof(MSDIServiceProviderAccessor), actual.GetType());
    }
}
