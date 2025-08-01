﻿using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.ClassNames;
using PeachtreeBus.Core.Tests.Sagas;
using PeachtreeBus.Core.Tests.Telemetry;
using PeachtreeBus.Data;
using PeachtreeBus.Exceptions;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using PeachtreeBus.Telemetry;
using PeachtreeBus.Testing;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Queues;

[TestClass]
public class QueuePipelineFinalStepFixture
{
    private readonly FakeServiceProviderAccessor _accessor = new(new());
    private readonly Mock<ILogger<QueuePipelineFinalStep>> _log = new();
    private readonly Mock<ISagaMessageMapManager> _sagaMessageMapManager = new();
    private readonly Mock<IQueueReader> _queueReader = new();
    private readonly ClassNameService _classNameService = new();

    private QueuePipelineFinalStep _testSubject = default!;
    private TestSaga _testSaga = default!;

    private SagaData? _sagaData = new()
    {
        SagaId = UniqueIdentity.New(),
        Blocked = false,
        Key = new("SagaKey"),
        Data = new("Data"),
        MetaData = TestData.CreateSagaMetaData(),
    };

    private IEnumerable<IHandleQueueMessage<TestSagaMessage1>> _handlers = [];

    private QueueContext Context = default!;

    [TestInitialize]
    public void Initialize()
    {
        _testSaga = new();

        _accessor.Reset();

        _log.Reset();
        _sagaMessageMapManager.Reset();
        _queueReader.Reset();

        _queueReader.Setup(x => x.LoadSaga(
            It.IsAny<object>(),
            It.IsAny<QueueContext>()))
            .Callback((object o, QueueContext c) =>
            {
                c.SagaData = _sagaData;
                c.SagaKey = _sagaData?.Key ?? new("SagaKey");
            });

        _accessor.Add(() => _handlers);

        Context = TestData.CreateQueueContext(
            userMessageFunc: () => new TestSagaMessage1(),
            messageData: TestData.CreateQueueData(
                headers: TestData.CreateHeaders(new TestSagaMessage1())));

        _testSubject = new(
            _accessor.Object,
            _log.Object,
            _sagaMessageMapManager.Object,
            _queueReader.Object,
            _classNameService);

        Assert.AreEqual(typeof(TestSagaMessage1), Context.Message.GetType(),
            "This test suite expects the default user message type to be TestSagaMessage1");
    }

    [TestMethod]
    public async Task Given_Handler_When_Invoke_Then_Activity()
    {
        _handlers = [new TestSaga()];
        using var listener = new TestActivityListener(ActivitySources.User);

        await _testSubject.Invoke(Context, null);

        var activity = listener.ExpectOneCompleteActivity();
        HandlerActivityFixture.AssertActivity(activity, typeof(TestSaga), Context);
    }

    [TestMethod]
    public async Task Given_MessageIsNotIQueuedMessage_Then_ThrowsUsefulException()
    {
        Context = TestData.CreateQueueContext(
            userMessageFunc: () => new object(),
            messageData: TestData.CreateQueueData(
                headers: new() { MessageClass = typeof(object).GetClassName() }));
        await Assert.ThrowsExactlyAsync<TypeIsNotIQueueMessageException>(() => _testSubject.Invoke(Context, null));
    }

    /// <summary>
    /// Proves behavior when the handler is a saga
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Given_MessageIsHandledBySaga_When_Invoke_Then_SagaHandlesMessage()
    {
        _handlers = [_testSaga];

        await _testSubject.Invoke(Context, null!);

        _queueReader.Verify(r => r.LoadSaga(It.IsAny<object>(), It.IsAny<QueueContext>()), Times.Once);
        _testSaga.AssertInvocations(1);
        _testSaga.AssertInvoked(Context, (TestSagaMessage1)Context.Message);
        _queueReader.Verify(r => r.SaveSaga(It.IsAny<object>(), It.IsAny<QueueContext>()), Times.Once);

        Assert.IsFalse(Context.SagaBlocked);
    }

    [TestMethod]
    public async Task Given_MultipleHandlersAndMultipleSagas_When_Invoke_Then_AllHandlersAreInvoked()
    {
        var saga1 = new TestSaga();
        var saga2 = new TestSaga();
        var handler1 = new Mock<IHandleQueueMessage<TestSagaMessage1>>();
        var handler2 = new Mock<IHandleQueueMessage<TestSagaMessage1>>();
        _handlers = [saga1, saga2, handler1.Object, handler2.Object];

        await _testSubject.Invoke(Context, null);

        saga1.AssertInvocations(1);
        saga1.AssertInvoked(Context, (TestSagaMessage1)Context.Message);
        saga2.AssertInvocations(1);
        saga2.AssertInvoked(Context, (TestSagaMessage1)Context.Message);
        handler1.Verify(h => h.Handle(Context, (TestSagaMessage1)Context.Message), Times.Once);
        handler2.Verify(h => h.Handle(Context, (TestSagaMessage1)Context.Message), Times.Once);

        _queueReader.Verify(r => r.LoadSaga(It.IsAny<object>(), It.IsAny<QueueContext>()), Times.Exactly(2));
        _queueReader.Verify(r => r.SaveSaga(It.IsAny<object>(), It.IsAny<QueueContext>()), Times.Exactly(2));
    }

    [TestMethod]
    public async Task Given_MultipleHanlders_When_Invoke_Then_MultipleActivities()
    {
        _handlers = [new TestSaga(), new TestSaga()];
        using var listener = new TestActivityListener(ActivitySources.User);

        await _testSubject.Invoke(Context, null);

        Assert.AreEqual(_handlers.Count(), listener.Stopped.Count);
    }

    /// <summary>
    /// Proves the behavior when the saga is blocked.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Given_SagaIsBlocked_When_Invoke_Then_Return()
    {
        _handlers = [_testSaga];
        _sagaData!.Blocked = true;

        await _testSubject.Invoke(Context, null);

        // the saga data will get loaded.
        _queueReader.Verify(r => r.LoadSaga(It.IsAny<object>(), It.IsAny<QueueContext>()), Times.Once);

        // the saga data will not be saved.
        _queueReader.Verify(r => r.SaveSaga(It.IsAny<object>(), It.IsAny<QueueContext>()), Times.Never);

        // a block will be reported.
        Assert.IsTrue(Context.SagaBlocked);

        // the saga handler should not be called.
        _testSaga.AssertInvocations(0);
    }

    [TestMethod]
    public async Task Given_SagaIsBlocked_When_Invoke_Then_ActivityIsUpdated()
    {
        _handlers = [_testSaga];
        _sagaData!.Blocked = true;
        using var listener = new TestActivityListener(ActivitySources.User);

        await _testSubject.Invoke(Context, null);

        var activity = listener.ExpectOneCompleteActivity();
        activity.AssertTag("peachtreebus.sagablocked", "true");
    }

    /// <summary>
    /// Proves behavior when a saga has not started yet.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Given_SagaNotStarted_And_MessageIsNotAStart_When_Invoke_Then_MessageIsNotHandled()
    {
        _handlers = [_testSaga];

        // returning null saga data means that the saga has not been started.
        _sagaData = null;

        // the handler for the message is not IHandleSagaStartMessage<>
        // therefore, handle should be skipped without error.
        await _testSubject.Invoke(Context, null);
    }

    /// <summary>
    /// Proves behavior when a message has no handlers.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Given_MessageHasNoHandlers_When_Invoke_Then_Throws()
    {
        _handlers = [];

        await Assert.ThrowsExactlyAsync<QueueMessageNoHandlerException>(() =>
            _testSubject.Invoke(Context, null));
    }

    /// <summary>
    /// Proves behavior when message type unrecognized.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Given_AMessageContextWithAnUnrecognizedMessageType_When_Invoke_Then_Throws()
    {
        var context = TestData.CreateQueueContext(
            messageData: TestData.CreateQueueData(
                headers: TestData.CreateHeadersWithUnrecognizedMessageClass()));
        await Assert.ThrowsExactlyAsync<QueueMessageClassNotRecognizedException>(() =>
            _testSubject.Invoke(context, null));
    }

    [TestMethod]
    public async Task Given_AMessageThatDoesNotImplementIQueueMessage_When_Invoke_Then_Throws()
    {
        var context = TestData.CreateQueueContext(
            userMessageFunc: () => new object(),
            messageData: TestData.CreateQueueData(
                headers: TestData.CreateHeaders(new object()))); // object does not implement IQueueMessage
        await Assert.ThrowsExactlyAsync<TypeIsNotIQueueMessageException>(() => _testSubject.Invoke(context, null));
    }

    [TestMethod]
    public async Task Given_FindHandlerReturnsNull_When_Invoke_Then_Throws()
    {
        _handlers = null!;

        await Assert.ThrowsExactlyAsync<QueueMessageNoHandlerException>(() =>
            _testSubject.Invoke(Context, null));
    }
}
