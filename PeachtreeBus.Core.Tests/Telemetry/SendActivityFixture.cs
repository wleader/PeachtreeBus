using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Telemetry;
using System.Diagnostics;
using System.Linq;

namespace PeachtreeBus.Tests.Telemetry;

[TestClass]
public class SendActivityFixture()
    : ActivityFixtureBase(ActivitySources.Messaging)
{
    [TestMethod]
    public void Given_SendContext_When_Activity_Then_TagsAreCorrect()
    {
        var context = TestData.CreateSendContext();
        new SendActivity(context).Dispose();

        AssertActivity(_listener.Stopped.SingleOrDefault(), context);
    }

    public static void AssertActivity(Activity? activity, SendContext context)
    {
        activity
            .AssertIsNotNull()
            .AssertOperationName("send " + context.Destination.ToString())
            .AssertKind(ActivityKind.Producer)
            .AssertOutgoingContext(context)
            .AssertDestination(context.Destination)
            .AssertStarted();
    }

    [TestMethod]
    public void Given_PublishContext_When_Activity_Then_TagsAreCorrect()
    {
        var context = TestData.CreatePublishContext();
        new SendActivity(context).Dispose();

        AssertActivity(_listener.Stopped.SingleOrDefault(), context);
    }

    public static void AssertActivity(Activity? activity, PublishContext context)
    {
        activity
            .AssertIsNotNull()
            .AssertOperationName("send " + context.Topic.ToString())
            .AssertKind(ActivityKind.Producer)
            .AssertOutgoingContext(context)
            .AssertDestination(context.Topic)
            .AssertStarted();
    }
}
