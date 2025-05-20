using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.SourceGenerators.LoggerExtensions;
using System.Text;

namespace PeachtreeBus.SourceGenerators.Tests.LoggerExtensions;

[TestClass]
public class NamespaceWriterFixture
{
    private NamespaceWriter _writer = default!;
    private readonly Mock<IState> _state = new();
    private readonly Mock<INamespaceBlocks> _blocks = new();
    private readonly Mock<IClassWriter> _classWriter = new();
    private NamespaceType _data = default!;

    [TestInitialize]
    public void Initialize()
    {
        _state.Reset();
        _blocks.Reset();
        _classWriter.Reset();

        _writer = new NamespaceWriter(
            _state.Object,
            _blocks.Object,
            _classWriter.Object);

        _data = new()
        {
            name = "namespace",
            namespaceId = 1,
            Class =
            [
                new(),
            ],
        };

    }

    [TestMethod]
    public void When_Write_Then_StateIsSetFirst()
    {
        bool stateSet = false;
        var sb = new StringBuilder();

        _state.Setup(s => s.SetNamespace(_data))
            .Callback(() => stateSet = true);

        _blocks.Setup(b => b.WriteBeforeClasses(sb, _data))
            .Callback(() => Assert.IsTrue(stateSet, "State was not set first."));

        _writer.Write(sb, _data);

        _state.Verify(s => s.SetNamespace(_data), Times.Once);
        _blocks.Verify(b => b.WriteBeforeClasses(sb, _data), Times.Once);
    }

    [TestMethod]
    public void When_Write_Then_BlocksWrittenInOrder()
    {
        var sb = new StringBuilder();

        _blocks.Setup(b => b.WriteBeforeClasses(sb, _data))
            .Callback(() => sb.Append("BeforeClasses"));

        _blocks.Setup(b => b.WriteAfterClasses(sb))
            .Callback(() => sb.Append("AfterClasses"));

        _classWriter.Setup(c => c.Write(sb, It.IsAny<ClassType>()))
            .Callback(() => sb.Append("Class"));

        _writer.Write(sb, _data);

        var actual = sb.ToString();

        Assert.AreEqual("BeforeClassesClassAfterClasses", actual);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(10)]
    public void When_Write_Then_EachNamespaceIsWritten(int classCount)
    {
        _data.Class = new ClassType[classCount];
        for (int i = 0; i < classCount; i++)
        {
            _data.Class[i] = new();
        }

        var sb = new StringBuilder();
        _writer.Write(sb, _data);

        for (int i = 0; i < classCount; i++)
        {
            _classWriter.Verify(b => b.Write(It.IsAny<StringBuilder>(), _data.Class[i]), Times.Once);
        }
    }
}
