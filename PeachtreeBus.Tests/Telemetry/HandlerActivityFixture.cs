using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Abstractions.Tests.TestClasses;
using PeachtreeBus.Queues;
using PeachtreeBus.Telemetry;
using System;
using System.Diagnostics;
using System.Linq;

namespace PeachtreeBus.Tests.Telemetry;

[TestClass]
public class HandlerActivityFixture()
    : ActivityFixtureBase(ActivitySources.User)
{
    [TestMethod]
    public void When_Activity_Then_TagsAreCorrect()
    {
        var context = TestData.CreateQueueContext();
        var handlerType = typeof(TestHandler);
        new HandlerActivity(handlerType, context).Dispose();
        Assert(_listener.Stopped.SingleOrDefault(), handlerType, context);
    }

    public static void Assert(Activity? activity, Type handlerType, QueueContext context) =>
        activity.AssertIsNotNull()
            .AssertOperationName("peachtreebus.handler " + handlerType.Name)
            .AssertKind(ActivityKind.Internal)
            .AssertHandlerType(handlerType)
            .AssertIncomingContext(context)
            .AssertStarted();
}
