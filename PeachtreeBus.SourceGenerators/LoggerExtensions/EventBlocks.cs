using System.Linq;
using System.Text;

namespace PeachtreeBus.SourceGenerators.LoggerExtensions;

public interface IEventBlocks
{
    void WriteEventId(StringBuilder writer);
    void WriteAction(StringBuilder writer, IEventData logMessage);
    void WriteExtension(StringBuilder writer, IEventData logMessage);
}

public class EventBlocks(
    IState state)
    : IEventBlocks
{
    public void WriteEventId(StringBuilder writer)
    {
        writer.Indent(2).Append("internal static readonly EventId ").Append(state.EventFullName).AppendLine("_Event");
        writer.Indent(3).Append("= new(").Append(state.CombinedId).Append(", \"").Append(state.EventFullName).AppendLine("\");");
        writer.AppendLine();
    }

    public void WriteAction(StringBuilder writer, IEventData logMessage)
    {
        writer.Indent(2).Append("internal static readonly Action<ILogger")
            .AppendEach(logMessage.Parameters, (p, b) => b.Append(", ").Append(p.TypeName))
            .Append(", Exception> ").Append(state.EventFullName).AppendLine("_Action");
        writer.Indent(3).Append("= LoggerMessage.Define")
            .AppendDefineTypes(logMessage.Parameters.Select(p => p.TypeName).ToList())
            .Append("(LogLevel.")
            .Append(logMessage.Level).AppendLine(",");
        writer.Indent(4).Append(state.EventFullName).AppendLine("_Event,");
        writer.Indent(4).Append('\"').Append(logMessage.MessageText).AppendLine("\");");
        writer.AppendLine();
    }

    public void WriteExtension(StringBuilder writer, IEventData eventData)
    {
        writer.Indent(2).AppendLine("/// <summary>");
        writer.Indent(2).Append("/// (").Append(state.CombinedId).Append(") ").Append(eventData.Level).Append(": ").AppendLine(eventData.MessageText);
        writer.Indent(2).AppendLine("/// </summary>");
        writer.Indent(2).Append("public static void ")
            .Append(eventData.Name)
            .Append(state.GenericArgs)
            .Append("(this ILogger<").Append(state.ClassName)
            .Append(state.GenericArgs)
            .Append("> logger")
            .AppendEach(eventData.Parameters, (p, b) => b.Append(", ").Append(p.TypeName).Append(' ').Append(p.LowerName))
            .Append(eventData.HasException ? ", Exception ex" : null)
            .Append(")").AppendLine(state.GenericConstraint);
        writer.Indent(3).Append("=> ").Append(state.EventFullName).Append("_Action(logger")
            .AppendEach(eventData.Parameters, (p, b) => b.Append(", ").Append(p.LowerName))
            .Append(eventData.HasException ? ", ex" : ", null!")
            .AppendLine(");");
    }
}
