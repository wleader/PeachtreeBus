using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.SourceGenerators.LoggerExtensions;
using System.Text;

namespace PeachtreeBus.SourceGenerators.Tests.LoggerExtensions;

[TestClass]
public class AssemblyBlocksFixture
{

    private AssemblyBlocks _blocks = default!;

    [TestInitialize]
    public void Initialize()
    {
        _blocks = new();
    }

    [TestMethod]
    [DataRow("Microsoft.Extensions.Logging")]
    [DataRow("System")]
    [DataRow("System.CodeDom.Compiler")]
    [DataRow("System.Collections.Generic")]
    [DataRow("System.Diagnostics.CodeAnalysis")]
    public void When_WriteHeader_Then_UsesNamespace(string expectedNamespace)
    {
        var sb = new StringBuilder();
        _blocks.WriteHeader(sb);
        var actual = sb.ToString();

        var expected = $"\r\nusing {expectedNamespace};\r\n";

        Assert.IsTrue(actual.Contains(expected));
    }

    [TestMethod]
    public void When_WriteNullableEnable_Then_Written()
    {
        var sb = new StringBuilder();
        _blocks.WriteEnableNullable(sb);
        var actual = sb.ToString();
        Assert.AreEqual("#nullable enable\r\n", actual);
    }

    [TestMethod]
    public void When_WriteUserUsings_Then_Written()
    {
        const string Expected = "using ns1;\r\nusing ns2;\r\nusing ns3;\r\n\r\n";
        var sb = new StringBuilder();
        _blocks.WriteUserUsings(sb, ["ns1", "ns2", "ns3"]);
        var actual = sb.ToString();
        Assert.AreEqual(Expected, actual);
    }
}
