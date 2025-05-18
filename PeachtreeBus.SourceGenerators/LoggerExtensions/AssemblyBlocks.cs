using System.Collections.Generic;
using System.Text;

namespace PeachtreeBus.SourceGenerators.LoggerExtensions;

public interface IAssemblyBlocks
{
    public void WriteHeader(StringBuilder writer);
    public void WriteUserUsings(StringBuilder writer, IEnumerable<string> usings);
    public void WriteEnableNullable(StringBuilder writer);
}

public class AssemblyBlocks : IAssemblyBlocks
{
    private const string Header = """
            //------------------------------------------------------
            // This is a generated file. Do not make manual changes.
            //------------------------------------------------------
            using Microsoft.Extensions.Logging;
            using System;
            using System.CodeDom.Compiler;
            using System.Collections.Generic;
            using System.Diagnostics.CodeAnalysis;
            """;

    public void WriteHeader(StringBuilder writer) => writer.AppendLine(Header);

    public void WriteUserUsings(StringBuilder writer, IEnumerable<string> usings)
    {
        foreach (var u in usings ?? [])
        {
            writer.Append("using ").Append(u).AppendLine(";");
        }
        writer.AppendLine();
    }

    public void WriteEnableNullable(StringBuilder writer) => writer.AppendLine("#nullable enable");
}
