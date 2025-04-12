using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Tasks;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Tasks;


[TestClass]
public class TaskManagerFixture
{
    private TaskManager _manager = default!;
    private readonly Mock<IProvideShutdownSignal> _shutdownSignal = new();
    private readonly Mock<IBusConfiguration> _busConfiguration = new();
    private readonly Mock<IStarters> _starters = new();
    private CancellationTokenSource _cts = default!;

    [TestInitialize]
    public void Initialize()
    {
        _shutdownSignal.Reset();
        _busConfiguration.Reset();
        _starters.Reset();

        _cts = new();

        _shutdownSignal.Setup(s => s.GetCancellationToken())
            .Returns(() => _cts.Token);

        _manager = new(
            _shutdownSignal.Object,
            _busConfiguration.Object,
            _starters.Object);
    }

    [TestMethod]
    public async Task When()
    {
        var t = Task.Run(_manager.Run);
        _cts.Cancel();
        await t;
    }

    [TestMethod]
    public void PortTests() => Assert.Inconclusive("Add Tests");
}
