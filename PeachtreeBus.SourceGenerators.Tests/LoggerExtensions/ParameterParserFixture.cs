using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.SourceGenerators.LoggerExtensions;

namespace PeachtreeBus.SourceGenerators.Tests.LoggerExtensions;

[TestClass]
public class ParameterParserFixture
{
    private ParameterParser _parser = default!;

    [TestInitialize]
    public void Intialize()
    {
        _parser = new();
    }

    [TestMethod]
    public void Given_MessageWithNoParameters_When_Parse_Then_EmptyList()
    {
        var actual = _parser.Parse("This messasge has no parameteres");
        Assert.AreEqual(0, actual.Count);
    }

    [TestMethod]
    public void Given_ParameterWithNoType_When_Parse_Then_EmptyList()
    {
        var actual = _parser.Parse("This messasge has a {Parameter} without a type");
        Assert.AreEqual(0, actual.Count);
    }

    [TestMethod]
    public void Given_ParameterWithType_When_Parse_Then_ParameterFound()
    {
        var actual = _parser.Parse("This messasge has a {Parameter:Type} with a type");
        Assert.AreEqual(1, actual.Count);
        var p = actual[0];
        Assert.AreEqual("Parameter", p.Name);
        Assert.AreEqual("parameter", p.LowerName);
        Assert.AreEqual("Type", p.TypeName);
        Assert.AreEqual("{Parameter:Type}", p.Substitution);
    }

    [TestMethod]
    public void Given_MultipleParameterWithType_When_Parse_Then_ParametersFound()
    {
        var actual = _parser.Parse("This messasge has a {Parameter:Type} with a type, and a {Second:string} parameter");
        Assert.AreEqual(2, actual.Count);
        var p = actual[0];
        Assert.AreEqual("Parameter", p.Name);
        Assert.AreEqual("parameter", p.LowerName);
        Assert.AreEqual("Type", p.TypeName);
        Assert.AreEqual("{Parameter:Type}", p.Substitution);

        p = actual[1];
        Assert.AreEqual("Second", p.Name);
        Assert.AreEqual("second", p.LowerName);
        Assert.AreEqual("string", p.TypeName);
        Assert.AreEqual("{Second:string}", p.Substitution);
    }

}
