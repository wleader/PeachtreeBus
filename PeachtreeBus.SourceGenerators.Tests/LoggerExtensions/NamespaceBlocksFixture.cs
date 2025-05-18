using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.SourceGenerators.LoggerExtensions;
using System.Text;

namespace PeachtreeBus.SourceGenerators.Tests.LoggerExtensions;

[TestClass]
public class NamespaceBlocksFixture
{
    private NamespaceBlocks _writer = default!;
    private readonly Mock<IState> _state = new();
    private bool _excludeFromCodeCoverage = false;
    private readonly NamespaceType _data = new()
    {
        name = "name.space",
    };

    [TestInitialize]
    public void Initialize()
    {
        _state.Reset();

        _state.SetupGet(s => s.ExcludeFromCodeCoverage).Returns(() => _excludeFromCodeCoverage);

        _writer = new(_state.Object);
    }

    [TestMethod]
    public void When_WriteAfterClasses_Then_Written()
    {
        var sb = new StringBuilder();
        _writer.WriteAfterClasses(sb);
        var actual = sb.ToString();
        Assert.AreEqual("    }\r\n}\r\n", actual);
    }

    [TestMethod]
    public void Given_ExcludeTrue_When_WriteBeforeClasses_Then_Written()
    {
        const string expected = """
            
            namespace name.space
            {
                [ExcludeFromCodeCoverage]
                [GeneratedCode("PeachtreeBus.SourceGenerators", "0.1")]
                internal static partial class GeneratedLoggerMessages
                {

            """;

        _excludeFromCodeCoverage = true;
        var sb = new StringBuilder();
        _writer.WriteBeforeClasses(sb, _data);
        var actual = sb.ToString();
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void Given_ExcludeFalse_When_WriteBeforeClasses_Then_Written()
    {
        const string expected =
            """
            
            namespace name.space
            {
                [GeneratedCode("PeachtreeBus.SourceGenerators", "0.1")]
                internal static partial class GeneratedLoggerMessages
                {

            """;

        _excludeFromCodeCoverage = false;
        var sb = new StringBuilder();
        _writer.WriteBeforeClasses(sb, _data);
        var actual = sb.ToString();
        Assert.AreEqual(expected, actual);
    }
}
