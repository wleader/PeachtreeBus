using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Abstractions.Tests.TestClasses;
using PeachtreeBus.Queues;
using PeachtreeBus.Telemetry;
using System;
using System.Diagnostics;
using System.Linq;

namespace PeachtreeBus.Tests.Telemetry;

[TestClass]
public class ActivityFactoryFixture
{
    private ActivityListener? _listener = default;
    private ActivityFactory _factory = default!;

    [TestInitialize]
    public void Initialize()
    {
        _factory = new();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _listener?.Dispose();
    }

    [TestMethod]
    [TestCategory("Trace Version")]
    public void Then_VersionDoesNotNeedToChange()
    {
        const string ChangeVersionMessage =
            "If the instruments change, then the Meter.Version must change.";
        const string Version = "0.11.0";

        // do not change these asserts unless there really is version change.
        Then_SourceHasNameAndKind(ActivityFactory.Messaging, "PeachtreeBus.Messaging", Version, ChangeVersionMessage);
        Then_SourceHasNameAndKind(ActivityFactory.User, "PeachtreeBus.User", Version, ChangeVersionMessage);
        Then_SourceHasNameAndKind(ActivityFactory.Internal, "PeachtreeBus.Internal", Version, ChangeVersionMessage);
    }

    [TestMethod]
    [TestCategory("Receive")]
    public void Given_Listener_When_Receive_Then_Activity()
    {
        Given_Listener();
        var start = DateTime.UtcNow;
        var context = TestData.CreateQueueContext(
            messageData: TestData.CreateQueueMessage(notBefore: start));

        var activity = _factory.Receive(context, start);

        Assert.IsNotNull(activity);
        Assert.AreEqual("receive " + context.SourceQueue, activity.OperationName);
        Assert.AreEqual(ActivityKind.Client, activity.Kind);
        Assert.AreEqual(start, activity.StartTimeUtc);
        Then_ActivityHasTag(activity, "messaging.system", "peachtreebus");
        Then_ActivityHasTag(activity, "messaging.operation.type", "receive");
        Then_ActivityHasTag(activity, "messaging.client.id", Environment.MachineName);
        Then_ActivityHasIncomingContextTags(activity, context);
        Then_ActivityHasDestinationTag(activity, context.SourceQueue.ToString());
        Then_ActivityIsStarted(activity);
    }

    [TestMethod]
    [TestCategory("PipelineStep")]
    public void Given_Listener_When_PipelineStep_Then_Activity()
    {
        Given_Listener();
        var pipeline = new TestQueuePipelineStep();
        var type = pipeline.GetType();

        var activity = _factory.PipelineStep(pipeline);
        Assert.IsNotNull(activity);
        Assert.AreEqual("peachtreebus.pipeline " + type.Name, activity.OperationName);
        Assert.AreEqual(ActivityKind.Internal, activity.Kind);
        Then_ActivityHasTag(activity, "peachtreebus.pipeline.type", type.FullName);
        Then_ActivityIsStarted(activity);
    }

    [TestMethod]
    [TestCategory("PipelineStep")]
    public void Given_Listener_And_PipelineStepIsFinalStep_When_PipelineStep_Then_ActivityIsNull()
    {
        // We don't want 'Final' steps to be presented to the end user
        // as regular pipeline steps.
        Given_Listener();
        Assert.IsNull(_factory.PipelineStep(new TestFinalStep()));
    }

    [TestMethod]
    [TestCategory("PipelineStep")]
    public void Given_Listener_And_StepIsNull_When_PipelineStep_Then_ActivityIsNull()
    {
        Given_Listener();
        Assert.IsNull(_factory.PipelineStep<IQueueContext>(null));
    }

    [TestMethod]
    [TestCategory("Handler")]
    public void Given_Listener_When_Handler_Then_Activity()
    {
        Given_Listener();
        var handler = new TestHandler();
        var context = TestData.CreateQueueContext();
        var activity = _factory.Handler(context, handler);
        activity = Then_ActivityHasHandlerTags(activity, handler);
        Then_ActivityHasIncomingContextTags(activity, context);

    }

    [TestMethod]
    [TestCategory("Handler")]

    public void Given_Listener_AndNullContext_When_Handler_Then_Activity()
    {
        Given_Listener();
        var handler = new TestHandler();
        var activity = _factory.Handler(null!, handler);
        Then_ActivityHasHandlerTags(activity, handler);
    }

    [TestMethod]
    [TestCategory("Handler")]

    public void Given_NoListener_When_Handler_Then_ActivityIsNull()
    {
        Assert.IsNull(_factory.Handler(null!, null!));
    }

    [TestMethod]
    [TestCategory("Send")]
    public void Given_Listener_When_Send_Then_Activity()
    {
        Given_Listener();
        var context = TestData.CreateSendContext();

        var activity = _factory.Send(context);

        Assert.IsNotNull(activity);
        Assert.AreEqual("send " + context.Destination.ToString(), activity.OperationName);
        Assert.AreEqual(ActivityKind.Producer, activity.Kind);
        Then_ActivityHasDestinationTag(activity, context.Destination.ToString());
        Then_ActivityHasOutgoingContextTags(activity, context);
    }

    [TestMethod]
    [TestCategory("Send")]
    public void Given_Listener_And_NotBeforeNull_When_Send_Then_Activity()
    {
        Given_Listener();
        var context = TestData.CreateSendContext();
        context.NotBefore = null;

        Assert.IsNotNull(_factory.Send(context));
    }

    private Activity Then_ActivityHasHandlerTags(Activity? activity, object handler)
    {
        Assert.IsNotNull(activity);
        Assert.AreEqual("peachtreebus.handler " + handler.GetType().Name, activity.OperationName);
        Assert.AreEqual(ActivityKind.Internal, activity.Kind);
        Then_ActivityHasTag(activity, "peachtreebus.handler.type", handler.GetType().FullName);
        Then_ActivityIsStarted(activity);
        return activity;
    }

    private static void Then_ActivityHasOutgoingContextTags(Activity activity, IOutgoingContext context)
    {
        Assert.IsTrue(context.NotBefore.HasValue, "Test context does not have a not before");
        Then_ActivityHasTag(activity, "peachtreebus.message.notbefore", context.NotBefore.Value.ToTagString());
        Then_ActivityHasTag(activity, "peachtreebus.message.priority", context.MessagePriority.ToString());
    }

    private static void Then_ActivityHasIncomingContextTags(Activity activity, IIncomingContext context)
    {
        Then_ActivityHasTag(activity, "messaging.message.id", context.MessageId.ToString());
        Then_ActivityHasTag(activity, "peachtreebus.message.notbefore", context.NotBefore.ToTagString());
        Then_ActivityHasTag(activity, "peachtreebus.message.priority", context.MessagePriority.ToString());
        Then_ActivityHasTag(activity, "peachtreebus.message.class", context.MessageClass);
        Then_ActivityHasTag(activity, "peachtreebus.message.enqueued", context.EnqueuedTime.ToTagString());
    }

    private static void Then_ActivityHasDestinationTag(Activity activity, string destination)
    {
        Then_ActivityHasTag(activity, "messaging.destination.name", destination);
    }

    private static void Then_ActivityHasTag(Activity activity, string tagName, string? expectedValue)
    {
        var tag = activity.Tags.SingleOrDefault(t => t.Key == tagName);
        // KeyValuePair is a struct, so it won't be null,
        // but it will have a null key when not found.
        Assert.AreEqual(tag.Key, tagName, $"Tag not found: {tagName}");
        Assert.AreEqual(expectedValue, tag.Value, "Tag value incorrect.");
    }

    private static void Then_SourceHasNameAndKind(ActivitySource source,
        string expectedName, string expectedVersion,
        string? message = default)
    {
        Assert.AreEqual(expectedName, source.Name, message);
        Assert.AreEqual(expectedVersion, source.Version, message);
    }

    private void Given_Listener()
    {
        static ActivitySamplingResult SampleAllData(
            ref ActivityCreationOptions<ActivityContext> options) =>
                ActivitySamplingResult.AllData;

        _listener = new()
        {
            ShouldListenTo = (s) => true,
            Sample = SampleAllData
        };
        ActivitySource.AddActivityListener(_listener);
    }

    private static void Then_ActivityIsStarted(Activity activity)
    {
        Assert.IsNotNull(activity);
        Assert.AreNotEqual(default, activity.StartTimeUtc);
        Assert.AreEqual(
            DateTime.UtcNow.Ticks,
            activity.StartTimeUtc.Ticks,
            TimeSpan.FromSeconds(1).Ticks);
    }
}
