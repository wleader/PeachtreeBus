using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Exceptions;
using System;

namespace PeachtreeBus.Core.Tests;

[TestClass]
public class ServiceProviderAccessorFixture
{
    private class Disposable : IDisposable
    {
        public int DisposeCount { get; private set; } = 0;
        public void Dispose()
        {
            DisposeCount++;
            GC.SuppressFinalize(this);
        }
    }

    private class TestableAccessor : ServiceProviderAccessor<Disposable>;

    private TestableAccessor _accessor = default!;
    private readonly Mock<IServiceProvider> _serviceProviderMock = new();
    private Disposable _disposable = default!;

    [TestInitialize]
    public void Initialize()
    {
        _serviceProviderMock.Reset();

        _disposable = new();
        _accessor = new();
    }

    [TestMethod]
    public void Given_Initialized_When_Dispose_Then_Disposed()
    {
        _accessor.Initialize(_disposable, _serviceProviderMock.Object);
        _accessor.Dispose();
        Assert.AreEqual(1, _disposable.DisposeCount);
    }

    [TestMethod]
    public void Given_Initialized_Then_IsConfiguredIsTrue()
    {
        _accessor.Initialize(_disposable, _serviceProviderMock.Object);
        Assert.IsTrue(_accessor.IsConfigured);
    }

    [TestMethod]
    public void Given_Intialized_When_GetServiceProvier_Then_ReturnsExisting()
    {
        _accessor.Initialize(_disposable, _serviceProviderMock.Object);
        Assert.AreSame(_serviceProviderMock.Object, _accessor.ServiceProvider);
    }

    [TestMethod]
    public void Given_UseExisting_When_Dispose_Then_DoesNotThrow()
    {
        _accessor.UseExisting(_serviceProviderMock.Object);
        _accessor.Dispose();
    }

    [TestMethod]
    public void Given_UseExisting_When_GetServiceProvider_Then_ReturnsExisting()
    {
        _accessor.UseExisting(_serviceProviderMock.Object);
        Assert.AreSame(_serviceProviderMock.Object, _accessor.ServiceProvider);
    }

    [TestMethod]
    public void Given_UseExisting_Then_IsConfiguredIsTrue()
    {
        _accessor.UseExisting(_serviceProviderMock.Object);
        Assert.IsTrue(_accessor.IsConfigured);
    }

    [TestMethod]
    public void Given_NotUseExisting_And_Not_Initialized_Then_IsConfiguredIsFalse()
    {
        Assert.IsFalse(_accessor.IsConfigured);
    }

    [TestMethod]
    public void Given_NotUseExisting_And_Not_Initialized_When_GetServiceProvder_Then_Throws()
    {
        Assert.ThrowsExactly<ServiceProviderAccessorException>(() => _ = _accessor.ServiceProvider);
    }
}
