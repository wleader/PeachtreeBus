using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.SourceGenerators.LoggerExtensions;
using System.Text;

namespace PeachtreeBus.SourceGenerators.Tests.LoggerExtensions;

[TestClass]
public class AssemblyBlocksFixture
{
    private AssemblyBlocks _blocks = null!;

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

        var expected = string.Format("{0}using {1};{0}", Environment.NewLine, expectedNamespace);

        Assert.IsTrue(actual.Contains(expected));
    }

    [TestMethod]
    public void When_WriteNullableEnable_Then_Written()
    {
        var sb = new StringBuilder();
        _blocks.WriteEnableNullable(sb);
        var actual = sb.ToString();
        var expected = $"#nullable enable{Environment.NewLine}";
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void When_WriteUserUsings_Then_Written()
    {
        var expected = string.Format("using ns1;{0}using ns2;{0}using ns3;{0}{0}", Environment.NewLine);
        var sb = new StringBuilder();
        _blocks.WriteUserUsings(sb, ["ns1", "ns2", "ns3"]);
        var actual = sb.ToString();
        Assert.AreEqual(expected, actual);
    }
}
