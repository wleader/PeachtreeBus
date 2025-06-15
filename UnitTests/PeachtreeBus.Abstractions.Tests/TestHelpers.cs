using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Exceptions;
using System;

namespace PeachtreeBus.Abstractions.Tests;

public static class TestHelpers
{
    private static readonly char[] DatabaseForbiddenChars =
    ['\\', ';', '@', '-', '/', '*', '\r', '\n', '\t', ' '];

    public static void AssertFunctionThrowsForDbUnsafeValues<T>(Func<string, T> func)
    {
        // DB safe names can not be null.
        Assert.ThrowsException<StringNotAllowedException>(() => func(null!));

        // DB safe names can not be empty strings 
        Assert.ThrowsException<StringNotAllowedException>(() => func(string.Empty));

        foreach (var c in DatabaseForbiddenChars)
        {
            Assert.ThrowsException<StringNotAllowedException>(() => func(c.ToString()));
        }
    }
}
