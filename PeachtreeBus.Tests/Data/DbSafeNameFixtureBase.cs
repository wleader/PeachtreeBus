using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using System;

namespace PeachtreeBus.Tests.Data;

public abstract class DbSafeNameFixtureBase
{
    private static readonly char[] ForbiddenChars =
        ['\\', ';', '@', '-', '/', '*', '\r', '\n', '\t', ' '];

    protected static void AssertActionThrowsForDbUnsafeValues(Action<string> action)
    {
        // DB safe names can not be null.
        Assert.ThrowsException<DbSafeNameException>(() => action(null!));

        // DB safe names can not be empty strings 
        Assert.ThrowsException<DbSafeNameException>(() => action(string.Empty));

        foreach (var c in ForbiddenChars)
        {
            Assert.ThrowsException<DbSafeNameException>(() => action(c.ToString()));
        }
    }
}
