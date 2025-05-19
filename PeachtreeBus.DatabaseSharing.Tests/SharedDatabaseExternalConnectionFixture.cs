using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PeachtreeBus.DatabaseSharing.Tests;

[TestClass]
public class SharedDatabaseExternalConnectionFixture : SharedDatabaseFixtureBase
{

    [TestInitialize]
    public override void Initialize()
    {
        base.Initialize();
    }

    [TestCleanup]
    public override void Cleanup()
    {
        base.Cleanup();
    }

    [TestMethod]
    public void Given_ExternalConnection_When()
    {
        Assert.Inconclusive("Tests not written.");
    }
}
