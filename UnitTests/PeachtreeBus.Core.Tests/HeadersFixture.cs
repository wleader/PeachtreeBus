using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.ClassNames;
using PeachtreeBus.Core.Tests.Sagas;

namespace PeachtreeBus.Core.Tests;

[TestClass]
public class HeadersFixture
{
    private static readonly ClassName TestSagaMessage1ClassName
        = new("PeachtreeBus.Core.Tests.Sagas.TestSagaMessage1, PeachtreeBus.Core.Tests");

    [TestMethod]
    public void Given_Type_When_New_Then_MessageClassInitialized()
    {
        var actual = new Headers() { MessageClass = typeof(TestSagaMessage1).GetClassName() };
        Assert.AreEqual(TestSagaMessage1ClassName, actual.MessageClass);
    }

    [TestMethod]
    public void When_New_Then_UserHeadersIntialized()
    {
        var actual = new Headers() { MessageClass = ClassName.Default };
        Assert.IsNotNull(actual?.UserHeaders);
        Assert.AreEqual(0, actual.UserHeaders.Count);
    }
}
