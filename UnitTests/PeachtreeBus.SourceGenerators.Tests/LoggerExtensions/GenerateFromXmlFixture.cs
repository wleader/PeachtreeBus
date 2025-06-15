using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.SourceGenerators.LoggerExtensions;

namespace PeachtreeBus.SourceGenerators.Tests.LoggerExtensions;

[TestClass]
public class GenerateFromXmlFixture
{
    private GenerateFromXml _generate = default!;
    private readonly AssemblyType _data = new();
    private readonly Mock<IXmlReader> _xmlReader = new();
    private readonly Mock<IAssemblyWriter> _generateFromData = new();

    [TestInitialize]
    public void Intialize()
    {
        _xmlReader.Reset();
        _generateFromData.Reset();

        _xmlReader.Setup(r => r.LoadXml("XML"))
            .Returns(() => _data);

        _generateFromData.Setup(g => g.Write(_data))
            .Returns("GENERATED");

        _generate = new(
            _xmlReader.Object,
            _generateFromData.Object);
    }

    [TestMethod]
    public void When()
    {
        var actual = _generate.FromXml("XML");
        Assert.AreEqual("GENERATED", actual);
    }
}
