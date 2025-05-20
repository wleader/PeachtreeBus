using System.Text;

namespace PeachtreeBus.SourceGenerators.LoggerExtensions;

public interface INamespaceBlocks
{
    void WriteBeforeClasses(StringBuilder writer, NamespaceType namespaceData);
    void WriteAfterClasses(StringBuilder writer);
}

public class NamespaceBlocks(
    IState state)
    : INamespaceBlocks
{
    public void WriteBeforeClasses(StringBuilder writer, NamespaceType namespaceData)
    {
        writer.AppendLine();
        writer.Append($"namespace ").AppendLine(namespaceData.name);
        writer.AppendLine("{");

        if (state.ExcludeFromCodeCoverage)
            writer.Indent(1).AppendLine("[ExcludeFromCodeCoverage]");

        writer.Indent(1).AppendLine("[GeneratedCode(\"PeachtreeBus.SourceGenerators\", \"0.1\")]");

        writer.Indent(1).AppendLine("internal static partial class GeneratedLoggerMessages");
        writer.Indent(1).AppendLine("{");
    }


    public void WriteAfterClasses(StringBuilder writer)
    {
        writer.Indent(1).AppendLine("}");
        writer.AppendLine("}");
    }
}


