using System.Collections.Generic;

namespace PeachtreeBus.SourceGenerators.LoggerExtensions;

public interface IEventData
{
    List<LogParameter> Parameters { get; }
    string MessageText { get; }
    string Level { get; }
    string Name { get; }
    bool HasException { get; }
}

public class EventData : IEventData
{
    public List<LogParameter> Parameters { get; set; } = default!;
    public string MessageText { get; set; } = default!;
    public string Level { get; set; } = default!;
    public string Name { get; set; } = default!;
    public bool HasException { get; set; }
}

public interface IEventTypeParser
{
    IEventData Parse(EventType eventData);
}

public class EventTypeParser(
    IParameterParser parameterParser,
    IMessageValidator validator)
    : IEventTypeParser
{
    public IEventData Parse(EventType eventData)
    {
        var value = eventData.Value.Trim();
        validator.Validate(value);
        var parameters = parameterParser.Parse(value);

        return new EventData()
        {
            Parameters = parameters,
            MessageText = GetMessageText(value, parameters),
            Level = eventData.levelSpecified ? eventData.level.ToString() : "Debug",
            Name = eventData.name,
            HasException = eventData.exceptionSpecified && eventData.exception,
        };
    }

    private string GetMessageText(string sourceMessage, List<LogParameter> parameters)
    {
        string result = sourceMessage;
        foreach (var p in parameters)
        {
            result = result.Replace(p.Substitution, "{" + p.Name + "}");
        }
        return result;
    }
}
