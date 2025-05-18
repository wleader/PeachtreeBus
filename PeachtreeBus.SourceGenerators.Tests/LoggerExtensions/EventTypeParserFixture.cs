using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.SourceGenerators.LoggerExtensions;
using System.Collections.Generic;

namespace PeachtreeBus.SourceGenerators.Tests.LoggerExtensions;

[TestClass]
public class EventTypeParserFixture
{
    private EventTypeParser _parser = default!;
    private readonly Mock<IParameterParser> _parameterParser = new();
    private readonly Mock<IMessageValidator> _validator = new();
    private EventType _data = default!;
    private List<LogParameter> _parameterParseResult = [];

    [TestInitialize]
    public void Initialize()
    {
        _parameterParser.Reset();
        _validator.Reset();

        _parameterParser.Setup(p => p.Parse(It.IsAny<string>()))
            .Returns(() => _parameterParseResult);

        _parser = new(
            _parameterParser.Object,
            _validator.Object);

        _data = new()
        {
            eventId = 12,
            exception = true,
            exceptionSpecified = true,
            level = LevelType.Warning,
            levelSpecified = true,
            name = "EventName",
            Value = "Message",
        };

    }

    [TestMethod]
    public void Given_NoParameters_When_Parse_Then_Result()
    {
        var actual = _parser.Parse(_data);
        Assert.IsNotNull(actual);
        Assert.AreSame(_parameterParseResult, actual.Parameters);
        Assert.IsTrue(actual.HasException);
        Assert.AreEqual("Warning", actual.Level);
        Assert.AreEqual("EventName", actual.Name);
        Assert.AreEqual("Message", actual.MessageText);
    }

    [TestMethod]
    public void Given_Parameters_When_Parse_Then_Result()
    {
        _data.Value = "This message has {Value1:string} and {Value2:int} parameters.";

        _parameterParseResult =
            [
                new() {Name = "Value1", LowerName="value1", TypeName = "string", Substitution = "{Value1:string}"},
                new() {Name = "Value2", LowerName="value2", TypeName = "int", Substitution = "{Value2:int}"},
            ];

        var actual = _parser.Parse(_data);
        Assert.IsNotNull(actual);
        Assert.AreSame(_parameterParseResult, actual.Parameters);
        Assert.IsTrue(actual.HasException);
        Assert.AreEqual("Warning", actual.Level);
        Assert.AreEqual("EventName", actual.Name);
        Assert.AreEqual("This message has {Value1} and {Value2} parameters.", actual.MessageText);
    }
}
