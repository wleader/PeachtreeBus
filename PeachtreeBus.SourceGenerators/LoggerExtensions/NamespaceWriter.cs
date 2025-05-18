using System.Text;

namespace PeachtreeBus.SourceGenerators.LoggerExtensions;

public interface INamespaceWriter
{
    void Write(StringBuilder writer, NamespaceType namespaceData);
}

public class NamespaceWriter(
    IState state,
    INamespaceBlocks blocks,
    IClassWriter classWriter)
    : INamespaceWriter
{
    public void Write(StringBuilder writer, NamespaceType namespaceData)
    {
        state.SetNamespace(namespaceData);
        blocks.WriteBeforeClasses(writer, namespaceData);
        foreach (var classData in namespaceData.Class ?? [])
        {
            classWriter.Write(writer, classData);
        }
        blocks.WriteAfterClasses(writer);
    }
}
