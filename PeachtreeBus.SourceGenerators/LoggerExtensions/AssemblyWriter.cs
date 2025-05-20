using System.Text;

namespace PeachtreeBus.SourceGenerators.LoggerExtensions;

public interface IAssemblyWriter
{
    string Write(AssemblyType data);
}

public class AssemblyWriter(
    IState state,
    IAssemblyBlocks blocks,
    INamespaceWriter namespaceWriter)
    : IAssemblyWriter
{
    public string Write(AssemblyType data)
    {
        state.Initialize(data);
        var writer = new StringBuilder();
        blocks.WriteHeader(writer);
        blocks.WriteUserUsings(writer, data.Usings);
        blocks.WriteEnableNullable(writer);
        foreach (var n in data.Namespace ?? [])
        {
            namespaceWriter.Write(writer, n);
        }
        return writer.ToString();
    }
}

