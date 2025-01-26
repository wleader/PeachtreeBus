using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Errors;
using System;

namespace PeachtreeBus.Tests;

[TestClass]
public class IncorrectImplementationExceptionFixture
{
    private static readonly Type ClassType = typeof(DefaultQueueRetryStrategy);
    private static readonly Type InterfaceType = typeof(IQueueRetryStrategy);

    [TestMethod]
    public void Given_Null_When_ThrowIfNull_Then_Throws()
    {
        object? parameter = null;
        var actual = Assert.ThrowsException<IncorrectImplementationException>(() =>
            IncorrectImplementationException.ThrowIfNull(parameter, ClassType, InterfaceType));

        Assert.IsNotNull(actual);
        Assert.IsNotNull(actual.Message);
        Assert.AreEqual(ClassType, actual.ClassType);
        Assert.AreEqual(InterfaceType, actual.InterfaceType);
    }

    [TestMethod]
    public void Given_Object_When_ThrowIfNull_Then_Result()
    {
        object? parameter = new();
        var actual = IncorrectImplementationException.ThrowIfNull(parameter, ClassType, InterfaceType);
        Assert.AreSame(parameter, actual);
    }

}
