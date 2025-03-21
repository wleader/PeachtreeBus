using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Exceptions;

namespace PeachtreeBus.Abstractions.Tests.Exceptions;

[TestClass]
public class UnreachableExceptionFixture
{
    [TestMethod]
    public void Given_Null_When_ThrowIfNull_Throws()
    {
        object? parameter = null;
        Assert.ThrowsException<UnreachableException>(() =>
            UnreachableException.ThrowIfNull(parameter));
    }

    [TestMethod]
    public void Given_Object_When_ThrowIfNull_ReturnsObject()
    {
        object? parameter = new();
        var actual = UnreachableException.ThrowIfNull(parameter);
        Assert.AreSame(parameter, actual);
    }
}
