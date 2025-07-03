using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PeachtreeBus.Testing.Tests;

[TestClass]
public class FakeServiceProviderAccessorFixture
{
    private FakeServiceProviderAccessor _accessor = default!;
    private readonly FakeServiceProvider _provider = new();

    [TestMethod]
    [DataRow(true, null, true, DisplayName = "ConstructFake_And_NotReset_ThenConfigured")]
    [DataRow(true, true, true, DisplayName = "ConstructFake_And_ResetObject_ThenConfigured")]
    [DataRow(true, false, true, DisplayName = "ConstructFake_And_ResetNull_ThenConfigured")]
    [DataRow(false, null, false, DisplayName = "ConstructNull_And_NotReset_ThenNotConfigured")]
    [DataRow(false, true, true, DisplayName = "ConstructNull_And_ResetObject_ThenConfigured")]
    [DataRow(false, false, false, DisplayName = "ConstructNull_And_ResetNull_ThenNotConfigured")]
    public void Given_Construct_When_Reset_Then_Configured(bool consutructFake, bool? reset, bool expectConfigured)
    {
        _accessor = new(consutructFake ? _provider : null);

        if (reset.HasValue)
            _accessor.Reset(reset.Value ? _provider.Object : null);

        Assert.AreEqual(expectConfigured, _accessor.Object.IsConfigured);

        if (expectConfigured)
        {
            Assert.AreSame(_provider.Object, _accessor.Object.ServiceProvider);
        }
        else
        {
            Assert.ThrowsExactly<FakeServiceProviderAcccessorException>(() =>
                _ = _accessor.Object.ServiceProvider);
        }
    }
}
