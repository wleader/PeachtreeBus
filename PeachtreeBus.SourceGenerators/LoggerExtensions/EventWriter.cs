using System.Text;

namespace PeachtreeBus.SourceGenerators.LoggerExtensions;

public interface IEventWriter
{
    void Write(StringBuilder writer, EventType eventElement);
}

public class EventWriter(
    IState state,
    IEventTypeParser parser,
    IEventBlocks blocks)
    : IEventWriter
{
    public void Write(StringBuilder writer, EventType eventElement)
    {
        state.SetEvent(eventElement);
        var eventData = parser.Parse(eventElement);
        blocks.WriteEventId(writer);
        blocks.WriteAction(writer, eventData);
        blocks.WriteExtension(writer, eventData);
    }
}
