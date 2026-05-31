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
        Assert.ThrowsExactly<StringNotAllowedException>(() => func(null!));

        // DB safe names can not be empty strings 
        Assert.ThrowsExactly<StringNotAllowedException>(() => func(string.Empty));

        foreach (var c in DatabaseForbiddenChars)
        {
            Assert.ThrowsExactly<StringNotAllowedException>(() => func(c.ToString()));
        }
    }
}
