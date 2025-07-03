using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Testing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PeachtreeBus.Core.Tests;

[TestClass]
public class ServiceProviderExtensionsFixture
{
    private interface ITestService;
    private class TestService : ITestService;

    private readonly FakeServiceProviderAccessor _fakeAccessor = new(new());
    private IServiceProviderAccessor _accessor = default!;
    private IServiceProvider _provider = default!;
    private IEnumerable<ITestService> _enumerable = [];
    private ITestService _service = new TestService();



    [TestInitialize]
    public void Initialize()
    {
        _fakeAccessor.Reset();
        _fakeAccessor.Add(() => _enumerable);
        _fakeAccessor.Add(() => _service);

        _accessor = _fakeAccessor.Object;
        _provider = _fakeAccessor.FakeProvider!.Object;
    }

    private static void Then_EmptyResult<T>(IEnumerable<T> actual)
    {
        Assert.IsNotNull(actual);
        Assert.IsFalse(actual.Any());
    }

    private static void Then_ResultIsEquivalent<T>(IEnumerable<T?> expeceted, IEnumerable<T?> actual)
    {
        CollectionAssert.AreEquivalent(expeceted.ToList(), actual.ToList());
    }

    [TestMethod]
    public void Given_EnumerableReturnsNull_When_GetServices_Then_EmptyResult()
    {
        _enumerable = null!;
        Then_EmptyResult(_provider.GetServices<ITestService>());
        Then_EmptyResult(_accessor.GetServices<ITestService>());
    }

    [TestMethod]
    public void Given_EnumerableReturnsEmpty_When_GetServices_Then_EmptyResult()
    {
        _enumerable = [];
        Then_EmptyResult(_provider.GetServices<ITestService>());
        Then_EmptyResult(_accessor.GetServices<ITestService>());
    }

    [TestMethod]
    public void Given_EnumerableReturnsSet_When_GetServices_Then_ResultIsSet()
    {
        _enumerable = [_service];
        Then_ResultIsEquivalent(_enumerable, _provider.GetServices<ITestService>());
        Then_ResultIsEquivalent(_enumerable, _accessor.GetServices<ITestService>());
    }

    [TestMethod]
    public void Given_ServiceReturnsNull_When_GetService_Then_ResultIsNull()
    {
        _service = null!;
        Assert.IsNull(_provider.GetService<ITestService>());
        Assert.IsNull(_accessor.GetService<ITestService>());
    }

    [TestMethod]
    public void Given_ServiceReturnsObject_When_GetService_Then_ResultIsObject()
    {
        _service = new TestService();
        var actual = _provider.GetService<ITestService>();
        Assert.AreEqual(_service, actual);

        Assert.AreSame(_service, _provider.GetService<ITestService>());
        Assert.AreSame(_service, _accessor.GetService<ITestService>());
    }

    [TestMethod]
    public void Given_ServiceReturnsNull_When_GetRequiredService_Then_Throws()
    {
        _service = null!;
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            _provider.GetRequiredService<ITestService>());
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            _accessor.GetRequiredService<ITestService>());
    }
}
