using System.Text;

namespace PeachtreeBus.SourceGenerators.LoggerExtensions;

public interface IClassWriter
{
    void Write(StringBuilder writer, ClassType classData);

}

public class ClassWriter(
    IState state,
    IEventWriter eventWriter)
    : IClassWriter
{
    public void Write(StringBuilder writer, ClassType classData)
    {
        state.SetClass(classData);
        bool addLine = false;
        foreach (var eventData in classData.Event ?? [])
        {
            if (addLine)
                writer.AppendLine();
            eventWriter.Write(writer, eventData);
            addLine = true;
        }
    }
}
