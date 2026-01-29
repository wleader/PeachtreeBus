using System.Text;

namespace PeachtreeBus.GenerateSql.Commands;

public static class BuilderExtensions
{
    public static StringBuilder IndentLine(this StringBuilder builder, string line, int indent) => 
        builder.Indent(indent).AppendLine(line);

    public static StringBuilder Indent(this StringBuilder builder, int indent) =>
        builder.Append(new string(' ', indent * 4));
}