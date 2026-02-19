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
        public int DisposeCount { get; private set; }
        public void Dispose()
        {
            DisposeCount++;
            GC.SuppressFinalize(this);
        }
    }

    private class TestableAccessor : ServiceProviderAccessor<Disposable>;

    private struct DisposableStruct : IDisposable
    {
        private readonly IDisposable _disposable;
        public int DisposeCount => _disposeCount;
        private int _disposeCount = 0;
        public DisposableStruct(IDisposable disposable)
        {
            _disposable = disposable;
        }

        public void Dispose()
        {
            _disposeCount++;
            _disposable.Dispose();
        }
    }

    private class TestableStructAccessor : ServiceProviderAccessor<DisposableStruct>
    {
        public DisposableStruct Value => _scope;
    }

    private TestableAccessor _accessor = null!;
    private readonly Mock<IServiceProvider> _serviceProviderMock = new();
    private Disposable _disposable = null!;

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
    public void Given_Initialized_When_GetServiceProvider_Then_ReturnsExisting()
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
    public void Given_NotUseExisting_And_Not_Initialized_When_GetServiceProvider_Then_Throws()
    {
        Assert.ThrowsExactly<ServiceProviderAccessorException>(() => _ = _accessor.ServiceProvider);
    }
    
    [TestMethod]
    public void Given_NotUseExisting_And_Not_Initialized_And_StructScope_When_Dispose_Then_DoesNotDispose()
    {
        var accessor = new TestableStructAccessor();
        accessor.Dispose();
        Assert.AreEqual(0, accessor.Value.DisposeCount);
    }
    
    [TestMethod]
    public void Given_Initialized_And_StructScope_When_Dispose_Then_Disposes()
    {
        var accessor = new TestableStructAccessor();
        accessor.Initialize(new(_disposable), _serviceProviderMock.Object);
        accessor.Dispose();
        Assert.AreEqual(1, _disposable.DisposeCount);
    }
    
}
