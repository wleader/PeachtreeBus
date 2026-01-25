using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeachtreeBus.SimpleInjector.Tests;

[TestClass]
public class ProvideShutdownCancellationTokenFixture
{
    private ProvideShutdownCancellationToken _provider = null!;

    [TestInitialize]
    public void Initialize()
    {
        _provider = new();
    }

    [TestMethod]
    public void When_GetCancellationToken_Then_TokenIsNotNull()
    {
        Assert.IsNotNull(_provider.GetCancellationToken());
    }

    [TestMethod]
    public void When_GetCancellationToken_Then_CancellationIsNotRequested()
    {
        var token = _provider.GetCancellationToken();
        Assert.IsFalse(token.IsCancellationRequested);
    }

    [TestMethod]
    public void Given_Token_When_SignalShutdown_Then_CancellationIsRequested()
    {
        var token = _provider.GetCancellationToken();
        _provider.SignalShutdown();
        Assert.IsTrue(token.IsCancellationRequested);
    }
}