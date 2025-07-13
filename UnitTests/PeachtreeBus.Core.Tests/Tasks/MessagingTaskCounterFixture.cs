using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Tasks;

namespace PeachtreeBus.Core.Tests.Tasks;

[TestClass]
public class MessagingTaskCounterFixture
{
    protected MessagingTaskCounter _counter = default!;
    protected Mock<IBusConfiguration> _busConfiguration = new();
    protected int _messageConcurrency = 0;

    [TestInitialize]
    public void Initialize()
    {
        _busConfiguration.Reset();
        _busConfiguration.SetupGet(c => c.MessageConcurrency)
            .Returns(() => _messageConcurrency);

        _counter = new(_busConfiguration.Object);
    }

    [TestMethod]
    [DataRow(32, 0, 32, DisplayName = "No Work")]
    [DataRow(32, 16, 16, DisplayName = "Some Work")]
    [DataRow(32, 5, 27, DisplayName = "Some Work 3")]
    [DataRow(32, 30, 2, DisplayName = "Some Work 3")]
    [DataRow(32, 32, 0, DisplayName = "Full Work")]
    [DataRow(32, 33, 0, DisplayName = "Over Full")]
    [DataRow(0, 1, 0, DisplayName = "No Capacity")]
    [DataRow(-1, 1, 0, DisplayName = "Negative Capacity")]
    public void Given_Concurrency_And_Value_Then_Available(int concurrency, int value, int expectedAvailable)
    {
        _messageConcurrency = concurrency;
        _counter.Value = value;
        Assert.AreEqual(expectedAvailable, _counter.Available());
    }
}
