﻿using PeachtreeBus.Exceptions;
using PeachtreeBus.Serialization;
using System;
using System.Text.Json.Serialization;

namespace PeachtreeBus.Data;

/// <summary>
/// A representation of a Transact-SQL UNIQUEIDENTIFIER
/// All Zeros is invalid.
/// </summary>
[JsonConverter(typeof(UniqueIdentityJsonConverter))]
public readonly record struct UniqueIdentity
{
    public Guid Value { get; }

    public UniqueIdentity(Guid value) : this(value, true) { }

    private UniqueIdentity(Guid value, bool validate)
    {
        if (validate)
            UniqueIdentityException.ThrowIfInvalid(value);
        Value = value;
    }

    public override string ToString() => Value.ToString();

    public static UniqueIdentity New() => new(Guid.NewGuid());

    public static readonly UniqueIdentity Empty = new(Guid.Empty, false);

    public class UniqueIdentityJsonConverter()
        : PeachtreeBusJsonConverter<UniqueIdentity, Guid>(l => new(l), i => i.Value);
}

public class UniqueIdentityException(string message) : PeachtreeBusException(message)
{
    public static void ThrowIfInvalid(Guid value)
    {
        if (value == Guid.Empty)
            throw new UniqueIdentityException(
                $"A UniqueIdentity cannot be {Guid.Empty}.");
    }
}

public static class UniqueIdentityExtensions
{
    public static UniqueIdentity RequireValid(this UniqueIdentity identity)
    {
        UniqueIdentityException.ThrowIfInvalid(identity.Value);
        return identity;
    }
}
