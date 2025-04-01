using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Tests.Sagas;

namespace PeachtreeBus.Tests;

[TestClass]
public class HeadersFixture
{
    private const string TestSagaMessage1ClassString
        = "PeachtreeBus.Tests.Sagas.TestSagaMessage1, PeachtreeBus.Core.Tests";

    [TestMethod]
    public void Given_Type_When_New_Then_MessageClassInitialized()
    {
        var actual = new Headers(typeof(TestSagaMessage1));
        Assert.AreEqual(TestSagaMessage1ClassString, actual.MessageClass);
    }

    [TestMethod]
    public void When_New_Then_UserHeadersIntialized()
    {
        var actual = new Headers();
        Assert.IsNotNull(actual?.UserHeaders);
        Assert.AreEqual(0, actual.UserHeaders.Count);
    }
}
