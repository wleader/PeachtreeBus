using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.SourceGenerators.LoggerExtensions;
using System.Text;

namespace PeachtreeBus.SourceGenerators.Tests.LoggerExtensions;

[TestClass]
public class ClassWriterFixture
{
    private ClassWriter _writer = default!;
    private readonly Mock<IState> _state = new();
    private readonly Mock<IEventWriter> _eventWriter = new();
    private ClassType _data = default!;

    [TestInitialize]
    public void Initialize()
    {
        _state.Reset();
        _eventWriter.Reset();

        _writer = new(
            _state.Object,
            _eventWriter.Object);

        _data = new()
        {
            Event =
            [
                new(),
            ],
        };
    }

    [TestMethod]
    public void When_Write_Then_StateIsSetFirst()
    {
        var sb = new StringBuilder();
        bool stateSet = false;

        _state.Setup(s => s.SetClass(_data))
            .Callback(() => stateSet = true);
        _eventWriter.Setup(e => e.Write(sb, It.IsAny<EventType>()))
            .Callback(() => Assert.IsTrue(stateSet, "State was not set first."));

        _writer.Write(sb, _data);

        _state.Verify(s => s.SetClass(_data), Times.Once);
        _eventWriter.Verify(e => e.Write(sb, _data.Event[0]), Times.Once);
    }


    [TestMethod]
    public void When_Write_Then_BlocksWrittenInOrder()
    {
        var sb = new StringBuilder();

        _eventWriter.Setup(e => e.Write(sb, It.IsAny<EventType>()))
            .Callback(() => sb.Append("Event"));

        _writer.Write(sb, _data);

        var actual = sb.ToString();

        Assert.AreEqual("Event", actual);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(10)]
    public void When_Write_Then_EachEventIsWritten(int eventCount)
    {
        _data.Event = new EventType[eventCount];
        for (int i = 0; i < eventCount; i++)
        {
            _data.Event[i] = new();
        }

        var sb = new StringBuilder();
        _writer.Write(sb, _data);

        for (int i = 0; i < eventCount; i++)
        {
            _eventWriter.Verify(b => b.Write(It.IsAny<StringBuilder>(), _data.Event[i]), Times.Once);
        }
    }

}
