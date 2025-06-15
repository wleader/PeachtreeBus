using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PeachtreeBus.DatabaseSharing.Tests;

[TestClass]
public class SharedDatabaseExceptionFixture
{
    [TestMethod]
    public void When_New_Then_MessageIsSet()
    {
        var ex = new SharedDatabaseException("CUSTOM MESSAGE");

        Assert.AreEqual("CUSTOM MESSAGE", ex.Message);
    }
}
