using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.ClassNames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.ClassNames;

[TestClass]
public class CachedClassNameServiceFixture
{
    private CachedClassNameService _service = default!;
    private readonly Mock<IClassNameService> _decorated = new();

    private static readonly ClassName Name1 = new("Name1");
    private static readonly ClassName Name2 = new("Name2");
    private static readonly Type Type1 = typeof(CachedClassNameService);
    private static readonly Type Type2 = typeof(CachedClassNameServiceFixture);
    private static readonly Dictionary<ClassName, Type> ToTypes = new()
    {
        { Name1, Type1 },
        { Name2, Type2 },
    };
    private static readonly Dictionary<Type, ClassName> ToNames = new()
    {
        { Type1, Name1 },
        { Type2, Name2 },
    };

    [TestInitialize]
    public void Initialize()
    {
        _decorated.Reset();

        _decorated.Setup(d => d.GetClassNameForType(It.IsAny<Type>()))
            .Returns((Type t) => ToNames[t]);
        _decorated.Setup(d => d.GetTypeForClassName(It.IsAny<ClassName>()))
            .Returns((ClassName c) => ToTypes[c]);

        _service = new(_decorated.Object);
    }

    [TestMethod]
    public void When_GetTypesMultipleTimes_Then_CachedResultIsUsed()
    {
        var actual = _service.GetTypeForClassName(Name1);
        Assert.AreEqual(Type1, actual);

        actual = _service.GetTypeForClassName(Name1);
        Assert.AreEqual(Type1, actual);

        actual = _service.GetTypeForClassName(Name2);
        Assert.AreEqual(Type2, actual);

        actual = _service.GetTypeForClassName(Name2);
        Assert.AreEqual(Type2, actual);

        // if it only called into the decorated class once,
        // it must have cached the result.
        _decorated.Verify(d => d.GetTypeForClassName(Name1), Times.Once());
        _decorated.Verify(d => d.GetTypeForClassName(Name2), Times.Once());
        _decorated.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void When_GetNameMultipleTimes_Then_CachedResultIsUsed()
    {
        var actual = _service.GetClassNameForType(Type1);
        Assert.AreEqual(Name1, actual);

        actual = _service.GetClassNameForType(Type1);
        Assert.AreEqual(Name1, actual);

        actual = _service.GetClassNameForType(Type2);
        Assert.AreEqual(Name2, actual);

        actual = _service.GetClassNameForType(Type2);
        Assert.AreEqual(Name2, actual);

        // if it only called into the decorated class once,
        // it must have cached the result.
        _decorated.Verify(d => d.GetClassNameForType(Type1), Times.Once());
        _decorated.Verify(d => d.GetClassNameForType(Type2), Times.Once());
        _decorated.VerifyNoOtherCalls();
    }
}
