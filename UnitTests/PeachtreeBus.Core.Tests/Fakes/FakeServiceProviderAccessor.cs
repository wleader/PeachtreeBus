using Moq;
using System;

namespace PeachtreeBus.Core.Tests.Fakes;

public class FakeServiceProviderAccessor : IServiceProviderAccessor
{
    public Mock<IServiceProviderAccessor> Mock = new();
    public Mock<IServiceProvider> ServiceProviderMock = new();
    private IServiceProvider? _userProvidedServiceProvider;

    public IServiceProvider ServiceProvider =>
        _userProvidedServiceProvider ??
        ServiceProviderMock.Object;

    public bool IsConfigured => true;

    public void Reset()
    {
        Mock.Reset();
        ServiceProviderMock.Reset();
        _userProvidedServiceProvider = null;
        Mock.SetupGet(m => m.ServiceProvider).Returns(() => ServiceProvider);
    }

    public void Dispose()
    {
        Mock.Object.Dispose();
        GC.SuppressFinalize(this);
    }

    public void UseExisting(IServiceProvider serviceProvider)
    {
        _userProvidedServiceProvider = serviceProvider;
    }

    public void SetupThrow<TService, TException>()
        where TException : Exception, new()
    {
        ServiceProviderMock.Setup(m => m.GetService(typeof(TService))).Throws<TException>();
    }

    public void SetupService<TService>(Func<TService> func)
    {
        ServiceProviderMock.Setup(m => m.GetService(typeof(TService))).Returns(func);
    }
}
