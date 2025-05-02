using PeachtreeBus.Exceptions;
using PeachtreeBus.Serialization;
using System;
using System.Text.Json.Serialization;

namespace PeachtreeBus.ClassNames;

[JsonConverter(typeof(ClassNameJsonConverter))]
public readonly record struct ClassName
{
    private readonly string _value;

    public string Value => _value
        ?? throw new NotInitializedException(typeof(ClassName));

    public ClassName(string value) : this(value, false) { }
    
    private ClassName(string? value, bool skipNullCheck = false)
    {
        if (!skipNullCheck)
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));
        _value = value!;
    }

    public override string ToString() => Value;

    public static readonly ClassName Uninitialized = new(null, true);
    public static readonly ClassName Default = new("System.Object, System.Private.CoreLib");

    public class ClassNameJsonConverter() 
        : PeachtreeBusJsonConverter<ClassName, string>(
            s => new(s, true), s => s.Value);
}
