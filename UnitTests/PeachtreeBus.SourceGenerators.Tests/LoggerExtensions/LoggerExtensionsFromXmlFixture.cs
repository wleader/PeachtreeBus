using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.SourceGenerators.LoggerExtensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace PeachtreeBus.SourceGenerators.Tests.LoggerExtensions;

[TestClass]
public sealed class LoggerExtensionsFromXmlFixture
{
    private LoggerExtensionsFromXml _generator = default!;
    private readonly Mock<IGenerateFromXml> _generateFromXml = new();
    private readonly List<AdditionalText> _texts = [];

    [TestInitialize]
    public void Initialize()
    {
        _generateFromXml.Reset();

        _generateFromXml.Setup(f => f.FromXml(It.IsAny<string>()))
            .Returns(() => "GENERATED");

        _texts.Clear();

        _generator = new(_generateFromXml.Object);
    }

    private GeneratorDriverRunResult When_Run()
    {
        var compilation = CSharpCompilation.Create("CSharpCodeGen.GenerateAssembly")
            .AddReferences()
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var texts = ImmutableArray.Create(_texts.ToArray());

        var driver = CSharpGeneratorDriver.Create(_generator)
            .AddAdditionalTexts(texts)
            .RunGeneratorsAndUpdateCompilation(compilation, out _, out var _);

        return driver.GetRunResult();
    }

    private void Given_AdditionalText()
    {
        _texts.Add(new InMemoryAdditionalText("c:\\path\\tp\\LogMessages.xml", "XMLDATA"));
    }

    [TestMethod]
    public void Given_AdditionalText_When_Run_Then_CodeIsGenerated()
    {
        Given_AdditionalText();

        var result = When_Run();

        _generateFromXml.Verify(f => f.FromXml("XMLDATA"), Times.Once);
        _generateFromXml.VerifyNoOtherCalls();

        // Verify the generated code
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.GeneratedTrees.Length);
        var generated = result.GeneratedTrees[0].ToString();
        Assert.AreEqual("GENERATED", generated);
    }

    [TestMethod]
    public void Given_AdditionalText_And_GeneratorThrows_When_Run_Then_OutputIsException()
    {
        Given_AdditionalText();

        _generateFromXml.Setup(f => f.FromXml(It.IsAny<string>()))
            .Throws<ApplicationException>();

        var result = When_Run();

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.GeneratedTrees.Length);
        Assert.AreEqual(1, result.Diagnostics.Length);

        var diagnostic = result.Diagnostics[0];
        Assert.AreEqual("PBSGEX01", diagnostic.Id);
    }
}
