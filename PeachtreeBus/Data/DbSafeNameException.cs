﻿using System.Linq;

namespace PeachtreeBus.Data;

public class DbSafeNameException : PeachtreeBusException
{
    public const string AllowedCharacters = "abcdefghijklmnopqrstuvwxyz0123456789";

    internal DbSafeNameException(string message) : base(message) { }

    public static void ThrowIfNotSafe(string value, string typeName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DbSafeNameException($"A {typeName} cannot be null, an empty string, or entirely whitespace.");

        if (value.ToLower().Any(c => !AllowedCharacters.Contains(c)))
            throw new DbSafeNameException($"A {typeName} can only contain the characters '{AllowedCharacters}'.");
    }
}
