using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Errors;
using PeachtreeBus.Tasks;
using System.Linq;
using System.Reflection;

namespace PeachtreeBus.Core.Tests.Errors;

[TestClass]
public class CircuitBreakerProviderFixture
{
    private CircuitBreakerProvider _provider = default!;
    private Mock<IDelayFactory> _delayFactory = new();
    private Mock<ILogger<CircuitBreaker>> _log = new();
    private Mock<IBusConfiguration> _busConfig = new();
    private Mock<ICircuitBreakerConfigurationProvider> _configProvider = new();
    private Mock<ISystemClock> _clock = new();

    private BreakerKey _key = new(BreakerType.DatabaseConnection, "CONNECTIONSTRING");
    private CircuitBreakerConfiguraton _config = new()
    {
        FriendlyName = "Freddy",
    };

    [TestInitialize]
    public void Initialize()
    {
        _delayFactory.Reset();
        _log.Reset();
        _busConfig.Reset();
        _configProvider.Reset();
        _clock.Reset();

        _configProvider.Setup(x => x.Get(_key))
            .Returns(() => _config);

        _busConfig.SetupGet(x => x.ConnectionString)
            .Returns("CONNECTIONSTRING");

        _provider = new(
            _delayFactory.Object,
            _log.Object,
            _busConfig.Object,
            _configProvider.Object,
            _clock.Object);
    }

    [TestMethod]
    public void When_GetBreaker_Then_Result_And_ConstructorParametersAreCorrect()
    {
        var actual = _provider.GetBreaker(_key);
        Assert.IsNotNull(actual);

        // verify constructor parameters.
        Assert.AreSame(_clock.Object, GetPrivateField<ISystemClock>(actual, "<clock>P"));
        Assert.AreSame(_log.Object, GetPrivateField<ILogger<CircuitBreaker>>(actual, "<log>P"));
        Assert.AreSame(_delayFactory.Object, GetPrivateField<IDelayFactory>(actual, "<delayFactory>P"));
        Assert.AreSame(_config, actual.Configuration);
    }

    [TestMethod]
    public void Then_BusDataConnectionKeyIsSetup()
    {
        Assert.AreEqual(_key, _provider.BusDataConnectionKey);
    }

    private T? GetPrivateField<T>(object instance, string name) where T : class
    {
        var type = instance.GetType();
        var fieldInfo = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(fieldInfo, $"Could not find private field '{name}'.");
        return fieldInfo.GetValue(instance) as T;
    }
}
