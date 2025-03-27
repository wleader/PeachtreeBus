using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Telemetry;
using System.Diagnostics;

namespace PeachtreeBus.Tests.Telemetry;

[TestClass]
public class ActivitySourcesFixture
{
    [TestMethod]
    public void Then_VersionDoesNotNeedToChange()
    {
        const string ChangeVersionMessage =
            "If the instruments change, then the Meter.Version must change.";
        const string Version = "0.11.0";

        // do not change these asserts unless there really is version change.
        AssertSource(ActivitySources.Messaging, "PeachtreeBus.Messaging", Version, ChangeVersionMessage);
        AssertSource(ActivitySources.User, "PeachtreeBus.User", Version, ChangeVersionMessage);
        AssertSource(ActivitySources.Internal, "PeachtreeBus.Internal", Version, ChangeVersionMessage);
        AssertSource(ActivitySources.Internal, "PeachtreeBus.DataAccess", Version, ChangeVersionMessage);
    }

    private static void AssertSource(ActivitySource source,
    string expectedName,
    string expectedVersion,
    string? message = default)
    {
        Assert.AreEqual(expectedName, source.Name, message);
        Assert.AreEqual(expectedVersion, source.Version, message);
    }
}
