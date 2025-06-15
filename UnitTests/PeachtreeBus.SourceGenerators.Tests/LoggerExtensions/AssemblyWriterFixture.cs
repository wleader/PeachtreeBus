using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.SourceGenerators.LoggerExtensions;
using System.Collections.Generic;
using System.Text;

namespace PeachtreeBus.SourceGenerators.Tests.LoggerExtensions;

[TestClass]
public class AssemblyWriterFixture
{
    private AssemblyWriter _writer = default!;
    private AssemblyType _data = default!;
    private readonly Mock<IState> _state = new();
    private readonly Mock<IAssemblyBlocks> _blocks = new();
    private readonly Mock<INamespaceWriter> _namespaceWriter = new();

    [TestInitialize]
    public void Initialize()
    {
        _state.Reset();
        _blocks.Reset();
        _namespaceWriter.Reset();

        _writer = new(
            _state.Object,
            _blocks.Object,
            _namespaceWriter.Object);

        _data = new()
        {
            // have at least one namespace so that
            // the write namespace gets called.
            Namespace = [new(),],
            Usings = [],
        };

    }

    [TestMethod]
    public void When_Write_Then_StateIsInitializedFirst()
    {
        bool stateInitialized = false;

        _state.Setup(s => s.Initialize(_data))
            .Callback(() => stateInitialized = true);

        _blocks.Setup(w => w.WriteHeader(It.IsAny<StringBuilder>()))
            .Callback(() => Assert.IsTrue(stateInitialized,
                "State was not initialized before writing."));

        _ = _writer.Write(_data);

        _blocks.Verify(w => w.WriteHeader(It.IsAny<StringBuilder>()), Times.Once);
        _state.Verify(s => s.Initialize(_data), Times.Once);
    }

    [TestMethod]
    public void When_Write_Then_BlocksWrittenInOrder()
    {
        _blocks.Setup(b => b.WriteHeader(It.IsAny<StringBuilder>()))
            .Callback((StringBuilder sb) => sb.Append("Header"));

        _blocks.Setup(b => b.WriteUserUsings(
            It.IsAny<StringBuilder>(),
            It.IsAny<IEnumerable<string>>()))
            .Callback((StringBuilder sb, IEnumerable<string> u) => sb.Append("Usings"));

        _blocks.Setup(b => b.WriteEnableNullable(It.IsAny<StringBuilder>()))
            .Callback((StringBuilder sb) => sb.Append("Nullable"));

        _namespaceWriter.Setup(b => b.Write(
            It.IsAny<StringBuilder>(),
            It.IsAny<NamespaceType>()))
            .Callback((StringBuilder sb, NamespaceType ns) => sb.Append("Namespace"));

        var actual = _writer.Write(_data);

        Assert.AreEqual("HeaderUsingsNullableNamespace", actual);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(10)]
    public void When_Write_Then_EachNamespaceIsWritten(int namespaceCount)
    {
        _data.Namespace = new NamespaceType[namespaceCount];
        for (int i = 0; i < namespaceCount; i++)
        {
            _data.Namespace[i] = new();
        }

        _ = _writer.Write(_data);

        for (int i = 0; i < namespaceCount; i++)
        {
            _namespaceWriter.Verify(b => b.Write(It.IsAny<StringBuilder>(), _data.Namespace[i]), Times.Once);
        }
    }
}
