using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Queues;
using PeachtreeBus.Telemetry;
using System.Diagnostics;
using System.Linq;

namespace PeachtreeBus.Tests.Telemetry;

[TestClass]
public class SendActivityFixture()
    : ActivityFixtureBase(ActivitySources.Messaging)
{
    [TestMethod]
    public void When_Activity_Then_TagsAreCorrect()
    {
        var context = TestData.CreateSendContext();
        new SendActivity(context).Dispose();

        Assert(_listener.Stopped.SingleOrDefault(), context);
    }

    public static void Assert(Activity? activity, SendContext context)
    {
        activity
            .AssertIsNotNull()
            .AssertOperationName("send " + context.Destination.ToString())
            .AssertKind(ActivityKind.Producer)
            .AssertOutgoingContext(context)
            .AssertDestination(context.Destination)
            .AssertStarted();
    }
}
