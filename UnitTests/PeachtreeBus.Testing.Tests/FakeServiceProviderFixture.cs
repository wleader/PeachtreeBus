using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace PeachtreeBus.Testing.Tests;

[TestClass]
public class FakeServiceProviderFixture
{
    private FakeServiceProvider _provider = default!;

    [TestInitialize]
    public void Initialize()
    {
        _provider = new();
    }

    [TestMethod]
    public void Given_NotRegistered_When_GetService_Then_Null()
    {
        Assert.IsNull(_provider.Object.GetService(typeof(IServiceProviderAccessor)));
    }

    [TestMethod]
    public void When_GetIServiceProvider_Then_ReturnsSelf()
    {
        Assert.AreSame(_provider.Object, _provider.Object.GetService(typeof(IServiceProvider)));
    }

    [TestMethod]
    public void Given_MockAdded_When_GetService_Then_ReturnsMock()
    {
        var added = _provider.AddMock<IServiceProviderAccessor>();
        Assert.AreSame(added.Object, _provider.Object.GetService(typeof(IServiceProviderAccessor)));
    }

    [TestMethod]
    public void Given_InstanceAdded_When_GetService_Then_ReturnsInstance()
    {
        var mock = new Mock<IServiceProviderAccessor>();
        _provider.Add(mock.Object);
        Assert.AreSame(mock.Object, _provider.Object.GetService(typeof(IServiceProviderAccessor)));
    }

    [TestMethod]
    public void Given_FuncAdded_When_GetService_Then_ReturnsInstance()
    {
        var mock = new Mock<IServiceProviderAccessor>();
        _provider.Add(() => mock.Object);
        Assert.AreSame(mock.Object, _provider.Object.GetService(typeof(IServiceProviderAccessor)));
    }

    [TestMethod]
    public void Given_MockAdded_When_GetRegistered_THen_ReturnsService()
    {
        var added = _provider.AddMock<IServiceProviderAccessor>();
        Assert.AreSame(added.Object, _provider.GetRegistered<IServiceProviderAccessor>());
    }

    [TestMethod]
    public void Given_InstanceAdded_When_GetRegistered_THen_ReturnsService()
    {
        var mock = new Mock<IServiceProviderAccessor>();
        _provider.Add(mock.Object);
        Assert.AreSame(mock.Object, _provider.GetRegistered<IServiceProviderAccessor>());
    }

    [TestMethod]
    public void Given_FuncAdded_When_GetRegistered_THen_ReturnsService()
    {
        var mock = new Mock<IServiceProviderAccessor>();
        _provider.Add(() => mock.Object);
        Assert.AreSame(mock.Object, _provider.GetRegistered<IServiceProviderAccessor>());
    }

    [TestMethod]
    public void Given_NotRegistered_When_GetRegistered_Then_Throws()
    {
        Assert.ThrowsExactly<FakeServiceProvderException>(() => _provider.GetRegistered<IServiceProviderAccessor>());
    }

    [TestMethod]
    public void Given_NotRegistered_When_GetMock_Then_Throws()
    {
        Assert.ThrowsExactly<FakeServiceProvderException>(() => _provider.GetMock<IServiceProviderAccessor>());
    }

    [TestMethod]
    public void Given_InstanceAdded_When_GetMock_Then_Throws()
    {
        var mock = new Mock<IServiceProviderAccessor>();
        _provider.Add(mock.Object);
        Assert.ThrowsExactly<FakeServiceProvderException>(() => _provider.GetMock<IServiceProviderAccessor>());
    }

    [TestMethod]
    public void Given_FuncAdded_When_GetMock_Then_Throws()
    {
        _provider.Add((Func<IServiceProviderAccessor>)null!);
        Assert.ThrowsExactly<FakeServiceProvderException>(() => _provider.GetMock<IServiceProviderAccessor>());
    }

    [TestMethod]
    public void Given_MockAdded_When_GetMock_Then_ReturnsMock()
    {
        var added = _provider.AddMock<IServiceProviderAccessor>();
        Assert.AreSame(added, _provider.GetMock<IServiceProviderAccessor>());
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(10)]
    public void Given_MockAdded_And_GetService_When_VerifyGetService_Then_Passes(int count)
    {
        _provider.AddMock<IServiceProviderAccessor>();
        for (int i = 0; i < count; i++)
        {
            _provider.Object.GetService(typeof(IServiceProviderAccessor));
        }
        _provider.VerifyGetService<IServiceProviderAccessor>(count);
    }

    [TestMethod]
    public void Given_MockAdded_And_NoGetService_When_VerifyGetService_Then_Fails()
    {
        _provider.AddMock<IServiceProviderAccessor>();
        Assert.ThrowsExactly<MockException>(() =>
            _provider.VerifyGetService<IServiceProviderAccessor>(1));
    }

    [TestMethod]
    public void Given_SetupThrow_When_GetService_Then_Throws()
    {
        _provider.SetupThrow<IServiceProviderAccessor, TestException>();
        Assert.ThrowsExactly<TestException>(() =>
            _provider.Object.GetService(typeof(IServiceProviderAccessor)));
    }

    [TestMethod]
    public void Given_SetupThrow_When_GetRegistered_Then_Throws()
    {
        _provider.SetupThrow<IServiceProviderAccessor, TestException>();
        Assert.ThrowsExactly<FakeServiceProvderException>(() =>
            _provider.GetRegistered<IServiceProviderAccessor>());
    }

    [TestMethod]
    public void Given_SetupThrow_When_GetMock_Then_Throws()
    {
        _provider.SetupThrow<IServiceProviderAccessor, TestException>();
        Assert.ThrowsExactly<FakeServiceProvderException>(() =>
            _provider.GetMock<IServiceProviderAccessor>());
    }
}
