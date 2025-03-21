﻿using PeachtreeBus.Exceptions;
using PeachtreeBus.Serialization;
using System.Text.Json.Serialization;

namespace PeachtreeBus.Data;

[JsonConverter(typeof(SchemaNameJsonConverter))]
public readonly record struct SchemaName
{
    private readonly string _value;

    public string Value => _value
        ?? throw new NotInitializedException(typeof(SchemaName));

    public SchemaName(string value)
    {
        DbSafeNameException.ThrowIfNotSafe(value, nameof(SchemaName));
        _value = value;
    }

    public override string ToString() => Value;

    public class SchemaNameJsonConverter()
        : PeachtreeBusJsonConverter<SchemaName, string>(s => new(s!), s => s.Value);
}
