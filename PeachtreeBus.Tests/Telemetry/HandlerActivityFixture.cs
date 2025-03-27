using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Abstractions.Tests.TestClasses;
using PeachtreeBus.Telemetry;
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

        _listener.Stopped.SingleOrDefault()
            .AssertIsNotNull()
            .AssertOperationName("peachtreebus.handler " + handlerType.Name)
            .AssertKind(ActivityKind.Internal)
            .AssertHandlerType(handlerType)
            .AssertIncomingContext(context)
            .AssertStarted();
    }
}

[TestClass]
public class SendActivityFixture()
    : ActivityFixtureBase(ActivitySources.Messaging)
{
    [TestMethod]
    public void When_Activity_Then_TagsAreCorrect()
    {
        var context = TestData.CreateSendContext();
        new SendActivity(context).Dispose();

        _listener.Stopped.SingleOrDefault()
            .AssertIsNotNull()
            .AssertOperationName("send " + context.Destination.ToString())
            .AssertKind(ActivityKind.Producer)
            .AssertOutgoingContext(context)
            .AssertDestination(context.Destination)
            .AssertStarted();
    }
}
