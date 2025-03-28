using System;
using Builder = System.Text.StringBuilder;

namespace PeachtreeBus;

public static class TypeNameExtensions
{
    public static string GetTypeFullName<T>(this T _) =>
        typeof(T).GetTypeFullName();

    public static string GetMessageClass<T>(this T _) =>
        typeof(T).GetMessageClass();

    public static string GetTypeFullName(this Type type) =>
        new Builder().AddTypeFullName(type).ToString();

    public static string GetMessageClass(this Type type) =>
        new Builder().AddMessageClass(type).ToString();

    private static Builder AddTypeFullName(this Builder bldr, Type type) =>
        type.IsGenericType ? bldr.AddGenericFullName(type) : bldr.AddBaseFullName(type);

    private static Builder AddMessageClass(this Builder bldr, Type type) =>
        bldr.AddTypeFullName(type).Append(", ").Append(type.Assembly.GetName().Name);

    private static Builder AddGenericFullName(this Builder bldr, Type type) =>
        type.IsGenericTypeDefinition
            ? bldr.AddBaseFullName(type.GetGenericTypeDefinition())
            : bldr.AddBaseFullName(type.GetGenericTypeDefinition())
                  .Append('[').AddArguments(type.GenericTypeArguments).Append(']');

    private static Builder AddArgument(this Builder bldr, Type type) =>
        bldr.Append('[').AddMessageClass(type).Append(']');

    private static Builder AddBaseFullName(this Builder bldr, Type type) =>
        type.IsNested
            ? bldr.AddTypeFullName(type.DeclaringType!).Append('+').Append(type.Name)
            : bldr.AddNamespace(type.Namespace).Append(type.Name);

    private static Builder AddNamespace(this Builder bldr, string? @namespace) =>
        @namespace is null ? bldr : bldr.Append(@namespace).Append('.');

    private static Builder AddArguments(this Builder bldr, Type[] args)
    {
        foreach (var arg in args[..^1]) { bldr.AddArgument(arg).Append(", "); }
        return bldr.AddArgument(args[^1]);
    }
}
