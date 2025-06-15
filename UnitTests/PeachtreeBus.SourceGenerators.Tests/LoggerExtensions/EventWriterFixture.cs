using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.SourceGenerators.LoggerExtensions;
using System.Text;

namespace PeachtreeBus.SourceGenerators.Tests.LoggerExtensions;

[TestClass]
public class EventWriterFixture
{
    private EventWriter _writer = default!;
    private readonly Mock<IState> _state = new();
    private readonly Mock<IEventTypeParser> _parser = new();
    private readonly Mock<IEventBlocks> _blocks = new();
    private EventType _element = default!;

    [TestInitialize]
    public void Intialize()
    {
        _state.Reset();
        _parser.Reset();
        _blocks.Reset();

        _writer = new(
            _state.Object,
            _parser.Object,
            _blocks.Object);

        _element = new()
        {
            eventId = 1,
            exception = true,
            exceptionSpecified = true,
            level = LevelType.Debug,
            levelSpecified = true,
            name = "name",
            Value = "value",
        };
    }

    [TestMethod]
    public void When_Write_Then_StateEventIsSetFirst()
    {
        bool stateSet = false;

        _state.Setup(s => s.SetEvent(_element))
            .Callback(() => stateSet = true);

        _parser.Setup(p => p.Parse(_element))
            .Callback(() => Assert.IsTrue(stateSet, "State was not set first."));

        var sb = new StringBuilder();
        _writer.Write(sb, _element);

        _state.Verify(s => s.SetEvent(_element), Times.Once);
        _parser.Verify(p => p.Parse(_element), Times.Once);
    }


    [TestMethod]
    public void When_Write_Then_BlocksAreWritten()
    {
        var data = new EventData();
        _parser.Setup(p => p.Parse(_element)).Returns(data);
        var sb = new StringBuilder();

        _blocks.Setup(b => b.WriteEventId(sb))
            .Callback((StringBuilder b) => b.Append("EventId"));

        _blocks.Setup(b => b.WriteAction(sb, data))
            .Callback((StringBuilder b, IEventData d) => b.Append("Action"));

        _blocks.Setup(b => b.WriteExtension(sb, data))
            .Callback((StringBuilder b, IEventData d) => b.Append("Extension"));

        _writer.Write(sb, _element);

        var actual = sb.ToString();
        Assert.AreEqual("EventIdActionExtension", actual);
    }
}
