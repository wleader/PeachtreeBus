using System;
using System.Collections.Generic;
using System.Text;

namespace PeachtreeBus.SourceGenerators.LoggerExtensions;

public static class StringBuilderExtensions
{
    public static StringBuilder AppendEach<T>(this StringBuilder builder, IEnumerable<T> value, Func<T, StringBuilder, StringBuilder> func)
    {
        var result = builder;
        foreach (var item in value) { result = func(item, result); }
        return result;
    }

    public static StringBuilder AppendDefineTypes<T>(this StringBuilder builder, IList<T> types)
    {
        if (types.Count < 1)
            return builder;
        return builder.Append('<').AppendJoin(", ", types).Append('>');
    }

    public static StringBuilder AppendJoin<T>(this StringBuilder builder, string delimiter, IList<T> values)
    {
        int count = values.Count - 1;
        if (count < 0)
            return builder;
        int i = 0;
        for (; i < count; i++)
        {
            builder.Append(values[i]).Append(delimiter);
        }
        builder.Append(values[i]);
        return builder;
    }

    public static StringBuilder Indent(this StringBuilder builder, int depth)
    {
        return builder.Append(' ', depth * 4);
    }
}

