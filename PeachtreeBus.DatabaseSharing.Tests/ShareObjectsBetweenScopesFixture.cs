using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace PeachtreeBus.DatabaseSharing.Tests;

[TestClass]
public class ShareObjectsBetweenScopesFixture
{
    [TestMethod]
    public void Given_SetSharedDatabase_When_GetSharedDatabase_Then_ResultMatches()
    {
        var sharedDb = new Mock<ISharedDatabase>().Object;
        var subject = new ShareObjectsBetweenScopes
        {
            SharedDatabase = sharedDb
        };
        Assert.AreSame(sharedDb, subject.SharedDatabase);
    }
}
