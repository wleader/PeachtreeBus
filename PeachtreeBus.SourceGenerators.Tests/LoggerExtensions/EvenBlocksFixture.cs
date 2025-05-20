using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.SourceGenerators.LoggerExtensions;
using System.Text;

namespace PeachtreeBus.SourceGenerators.Tests.LoggerExtensions;

[TestClass]
public class EvenBlocksFixture
{
    private EventBlocks _blocks = default!;
    private readonly Mock<IState> _state = new();
    private const string _combinedId = "1234567890";
    private const string _eventFullName = "EventFullName";
    private EventData _eventData = default!;

    [TestInitialize]
    public void Initialize()
    {
        _state.Reset();

        _state.SetupGet(s => s.CombinedId).Returns(() => _combinedId);
        _state.SetupGet(s => s.EventFullName).Returns(() => _eventFullName);

        _blocks = new(_state.Object);

        _eventData = new()
        {
            Parameters = [],
            Level = "Debug",
            MessageText = "Hello World"
        };
    }

    [TestMethod]
    public void When_WriteEventId_Then_Written()
    {
        var sb = new StringBuilder();
        _blocks.WriteEventId(sb);
        var actual = sb.ToString();

        //string expected = "        internal static readonly EventId EventFullName_Event\r\n            = new(1234567890, \"EventFullName\");\r\n\r\n";
        string expected = """
                    internal static readonly EventId EventFullName_Event
                        = new(1234567890, "EventFullName");


            """;

        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void Given_NoParameters_When_WriteAction_Then_Written()
    {
        var sb = new StringBuilder();
        _blocks.WriteAction(sb, _eventData);
        var actual = sb.ToString();

        string expected = """
                    internal static readonly Action<ILogger, Exception> EventFullName_Action
                        = LoggerMessage.Define(LogLevel.Debug,
                            EventFullName_Event,
                            "Hello World");


            """;

        Assert.AreEqual(expected, actual);

    }

    [TestMethod]
    public void Given_Parameters_When_WriteAction_Then_Written()
    {
        _eventData.Parameters =
            [
             new() {TypeName = "string"},
             new() {TypeName = "int"},
            ];

        var sb = new StringBuilder();
        _blocks.WriteAction(sb, _eventData);
        var actual = sb.ToString();

        string expected = """
                    internal static readonly Action<ILogger, string, int, Exception> EventFullName_Action
                        = LoggerMessage.Define<string, int>(LogLevel.Debug,
                            EventFullName_Event,
                            "Hello World");


            """;

        Assert.AreEqual(expected, actual);

    }

    [TestMethod]
    public void Given_NoParameters_When_WriteExtension_Then_Written()
    {
        var sb = new StringBuilder();
        _blocks.WriteExtension(sb, _eventData);

        var actual = sb.ToString();

        string expected = """
                    /// <summary>
                    /// (1234567890) Debug: Hello World
                    /// </summary>
                    public static void (this ILogger<> logger)
                        => EventFullName_Action(logger, null!);

            """;

        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void Given_Parameters_When_WriteExtension_Then_Written()
    {
        _eventData.Parameters =
            [
             new() {LowerName = "variable1", Name = "Variable1", TypeName = "string"},
             new() {LowerName = "variable2", Name = "Variable2", TypeName = "int"},
            ];

        var sb = new StringBuilder();
        _blocks.WriteExtension(sb, _eventData);

        var actual = sb.ToString();

        string expected = """
                    /// <summary>
                    /// (1234567890) Debug: Hello World
                    /// </summary>
                    public static void (this ILogger<> logger, string variable1, int variable2)
                        => EventFullName_Action(logger, variable1, variable2, null!);

            """;

        Assert.AreEqual(expected, actual);
    }
}
