using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using System;

namespace PeachtreeBus.Tests.Data;

public abstract class DbSafeNameFixtureBase
{
    private static readonly char[] ForbiddenChars =
        ['\\', ';', '@', '-', '/', '*', '\r', '\n', '\t', ' '];

    protected static void AssertFunctionThrowsForDbUnsafeValues<T>(Func<string, T> func)
    {
        // DB safe names can not be null.
        Assert.ThrowsException<DbSafeNameException>(() => func(null!));

        // DB safe names can not be empty strings 
        Assert.ThrowsException<DbSafeNameException>(() => func(string.Empty));

        foreach (var c in ForbiddenChars)
        {
            Assert.ThrowsException<DbSafeNameException>(() => func(c.ToString()));
        }
    }
}
