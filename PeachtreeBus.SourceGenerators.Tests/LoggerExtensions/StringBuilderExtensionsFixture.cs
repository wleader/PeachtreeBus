using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.SourceGenerators.LoggerExtensions;
using System.Collections.Generic;
using System.Text;

namespace PeachtreeBus.SourceGenerators.Tests.LoggerExtensions;

[TestClass]
public class StringBuilderExtensionsFixture
{
    private StringBuilder _builder = default!;

    [TestInitialize]
    public void Initialize()
    {
        _builder = new();
    }

    [TestMethod]
    [DataRow(0, "", DisplayName = "Indent0")]
    [DataRow(1, "    ", DisplayName = "Indent1")]
    [DataRow(2, "        ", DisplayName = "Indent2")]
    [DataRow(3, "            ", DisplayName = "Indent3")]
    public void Given_Value_When_Indent_Then_Indented(int value, string expected)
    {
        var actual = _builder.Indent(value).ToString();
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [DataRow(0, "", DisplayName = "Join0")]
    [DataRow(1, "1", DisplayName = "Join1")]
    [DataRow(2, "1,2", DisplayName = "Join2")]
    [DataRow(3, "1,2,3", DisplayName = "Join3")]
    [DataRow(10, "1,2,3,4,5,6,7,8,9,10", DisplayName = "Join10")]
    public void Given_Count_When_AppendJoin_Then_Result(int count, string expected)
    {
        var list = new List<string>();
        for (int i = 1; i <= count; i++)
            list.Add(i.ToString());

        // call it explicitly because the generator is .net standard 2.0 which doesn't have append join,
        // but the test are .net8 which does. 
        var actual = StringBuilderExtensions.AppendJoin(_builder, ",", list).ToString();
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [DataRow(0, "", DisplayName = "DefineTypes0")]
    [DataRow(1, "<1>", DisplayName = "DefineTypes1")]
    [DataRow(2, "<1, 2>", DisplayName = "DefineTypes2")]
    [DataRow(3, "<1, 2, 3>", DisplayName = "DefineTypes3")]
    [DataRow(10, "<1, 2, 3, 4, 5, 6, 7, 8, 9, 10>", DisplayName = "DefineTypes10")]
    public void Given_Count_When_AppendDefineTypes_Then_Result(int count, string expected)
    {
        var list = new List<string>();
        for (int i = 1; i <= count; i++)
            list.Add(i.ToString());

        var actual = _builder.AppendDefineTypes(list).ToString();
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [DataRow(0, "", DisplayName = "Each0")]
    [DataRow(1, "1", DisplayName = "Each1")]
    [DataRow(2, "12", DisplayName = "Each2")]
    [DataRow(3, "123", DisplayName = "Each3")]
    [DataRow(10, "12345678910", DisplayName = "Each10")]
    public void Given_Count_When_AppendEach_Then_Result(int count, string expected)
    {
        var list = new List<string>();
        for (int i = 1; i <= count; i++)
            list.Add(i.ToString());

        var actual = _builder.AppendEach<string>(list, (s, b) => b.Append(s)).ToString();
        Assert.AreEqual(expected, actual);
    }
}
